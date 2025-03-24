using System.Collections.Generic;
using UnityEngine;

namespace RougueBit.Play.Interface
{
    public interface IStageGeneratable
    {
        public void Generate();
        public Vector2 GetRandomFloor();
    }
}
