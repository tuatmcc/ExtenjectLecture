using System;
using UnityEngine;

namespace RougueBit.Play.Interface
{
    public interface IPlayManager
    {
        public event Action<PlayState> OnPlayStateChanged;
        public PlayState PlayState { get; }
        public PlayInputs PlayInputs { get; }
    }
}

