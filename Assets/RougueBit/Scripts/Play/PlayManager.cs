using RougueBit.Play.Interface;
using System;
using UnityEngine;
using Zenject;

namespace RougueBit.Play
{
    public class PlayManager : IPlayManager, IInitializable, IDisposable
    {
        public PlayInputs PlayInputs { get; } = new();

        private readonly StageGenerator stageGenerator;

        [Inject]
        public PlayManager(PlaySceneSO playSceneSO)
        {
            stageGenerator = new(playSceneSO);
            PlayInputs.Enable();
        }

        public void Initialize()
        {
            stageGenerator.Generate();
        }

        public void Dispose()
        {
            PlayInputs.Dispose();
        }
    }
}
