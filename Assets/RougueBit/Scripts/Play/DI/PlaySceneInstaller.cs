using RougueBit.Play.Tests;
using UnityEngine;
using Zenject;

namespace RougueBit.Play.DI
{
    public class PlaySceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<TestPlayManager>().AsSingle();
        }
    }
}