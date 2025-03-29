using R3;
using RougueBit.Play.Interface;
using System;
using UnityEngine;
using Zenject;

namespace RougueBit.Play
{
    public class PlayManager : IPlayManager, IInitializable, IDisposable
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
        private PlaySceneSO playSceneSO;
        private readonly IStageGeneratable stageGenerator;
        private CompositeDisposable disposables = new();

        [Inject]
        public PlayManager(PlaySceneSO playSceneSO)
        {
            this.playSceneSO = playSceneSO;
            stageGenerator = new StageGenerator(playSceneSO);
            PlayInputs.Enable();
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
            switch(nextState)
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
            PlayInputs.Dispose();
        }
    }
}
