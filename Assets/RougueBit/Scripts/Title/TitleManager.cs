using R3;
using RougueBit.Core;
using System;
using UnityEngine.InputSystem;
using Zenject;

namespace RougueBit.Title
{
    public class TitleManager: ITitleManager, IInitializable, IDisposable
    {
        [Inject] private readonly IGameStateManager _gameStateManager;

        public TitleInputs TitleInputs { get; private set; } = new();

        private CompositeDisposable disposables = new();

        public TitleManager()
        {
            TitleInputs.Enable();
        }

        public void Initialize()
        {
            Observable.FromEvent<InputAction.CallbackContext>(
                h => TitleInputs.Main.Enter.performed += h,
                h => TitleInputs.Main.Enter.performed -= h
            ).Subscribe(_ => _gameStateManager.NextScene()).AddTo(disposables);
        }

        public void Dispose()
        {
            disposables.Dispose();
            TitleInputs.Disable();
        }
    }
}
