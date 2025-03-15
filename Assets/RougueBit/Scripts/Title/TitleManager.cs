using R3;
using RougueBit.Core;
using System;
using UnityEngine.InputSystem;
using Zenject;

namespace RougueBit.Title
{
    public class TitleManager: ITitleManager, IInitializable, IDisposable
    {
        [Inject] private readonly GameStateManager _gameStateManager;

        public TitleInputs TitleInputs { get; private set; } = new();

        public TitleManager()
        {
            TitleInputs.Enable();
        }

        public void Initialize()
        {
            Observable.FromEvent<InputAction.CallbackContext>(
                h => TitleInputs.Main.Enter.performed += h,
                h => TitleInputs.Main.Enter.performed -= h
            ).Subscribe(_ => _gameStateManager.NextScene());
        }

        public void Dispose()
        {
            TitleInputs.Disable();
        }
    }
}
