using RougueBit.Play.Tests;
using UnityEngine;
using Zenject;

namespace RougueBit.Play.DI
{
    public class PlaySceneInstaller : MonoInstaller
    {
        [SerializeField] private bool isTest;

        public override void InstallBindings()
        {
            if (isTest)
            {
                Container.BindInterfacesTo<TestPlayManager>().AsSingle();
            }
            else
            {
                Container.BindInterfacesTo<PlayManager>().AsSingle();
            }
        }
    }
}
