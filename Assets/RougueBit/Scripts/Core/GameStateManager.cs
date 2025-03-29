using R3;
using System;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Zenject;

namespace RougueBit.Core
{
    public class GameStateManager: IGameStateManager, IInitializable, IDisposable
    {
        public event Action<GameState> OnGameStateChanged;

        public GameState GameState
        {
            get => _gameState;
            private set
            {
                if (_gameState == value)
                {
                    return;
                }
                _gameState = value;
                OnGameStateChanged?.Invoke(_gameState);
            }
        }
        private GameState _gameState;

        public CoreInputs CoreInputs { get; set; } = new();

        private CompositeDisposable disposables = new();

        // Awakeに相当
        public GameStateManager()
        {
            GameState = GameState.Title;
            CoreInputs.Enable();
        }

        // Startに相当
        public void Initialize()
        {
            GameState = GameState.Title;
            Observable.FromEvent<InputAction.CallbackContext>(
                h => CoreInputs.Main.Reset.performed += h,
                h => CoreInputs.Main.Reset.performed -= h
            ).Subscribe(_ => Reset()).AddTo(disposables);
            Observable.FromEvent<GameState>(
                h => OnGameStateChanged += h,
                h => OnGameStateChanged -= h
            ).Subscribe(TransitScene).AddTo(disposables);
        }

        public void NextScene()
        {
            switch (GameState)
            {
                case GameState.Title:
                    GameState = GameState.Playing;
                    break;
                case GameState.Playing:
                    GameState = GameState.Result;
                    break;
                case GameState.Result:
                    Reset();
                    break;
            }
        }

        private void TransitScene(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.Title:
                    SceneManager.LoadScene("Title");
                    break;
                case GameState.Playing:
                    SceneManager.LoadScene("Play");
                    break;
                case GameState.Result:
                    SceneManager.LoadScene("Result");
                    break;
            }
        }

        private void Reset()
        {
            GameState = GameState.Title;
        }

        // OnDestroyに相当
        public void Dispose()
        {
            disposables.Dispose();
            CoreInputs.Dispose();
        }
    }
}
