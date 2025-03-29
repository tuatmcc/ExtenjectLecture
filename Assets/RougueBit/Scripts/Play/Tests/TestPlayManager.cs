using R3;
using RougueBit.Play.Interface;
using System;
using UnityEngine;
using Zenject;

namespace RougueBit.Play.Tests
{
    public class TestPlayManager : IPlayManager, IInitializable, IDisposable
    {
        public event Action<PlayState> OnPlayStateChanged;

        public PlayState PlayState
        {
            get => playState;
            private set
            {
                playState = value;
                OnPlayStateChanged?.Invoke(playState);
            }
        }


        public PlayInputs PlayInputs { get; } = new();

        private PlayState playState;
        private IStageGeneratable stageGenerator;
        private PlaySceneSO playSceneSO;
        private CompositeDisposable disposables = new();

        [Inject]
        public TestPlayManager(PlaySceneSO playSceneSO)
        {
            this.playSceneSO = playSceneSO;
            PlayInputs.Enable();
            stageGenerator = new TestStageGenerator(playSceneSO);
        }

        public void Initialize()
        {
            Observable.FromEvent<PlayState>(
                h => OnPlayStateChanged += h,
                h => OnPlayStateChanged -= h
            ).Subscribe(NextState).AddTo(disposables);
            PlayState = PlayState.GenerateStage;
        }

        private void NextState(PlayState nextState)
        {
            switch (nextState)
            {
                case PlayState.GenerateStage:
                    stageGenerator.Generate();
                    playSceneSO.PlayerStartPosition = stageGenerator.GetRandomFloor();
                    PlayState = PlayState.SetPlayer;
                    break;
                case PlayState.SetPlayer:
                    break;
                case PlayState.Playing:
                    break;
            }
        }

        public void Dispose()
        {
            disposables.Dispose();
            PlayInputs.Disable();
        }
    }
}
