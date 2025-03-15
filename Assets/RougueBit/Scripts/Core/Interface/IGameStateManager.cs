using System;

namespace RougueBit.Core
{
    public interface IGameStateManager
    {
        public event Action<GameState> OnGameStateChanged;
        public GameState GameState { get; }
        public CoreInputs CoreInputs { get; }
        public void NextScene();
    }
}
