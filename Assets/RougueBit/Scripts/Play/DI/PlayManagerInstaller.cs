using UnityEngine;
using Zenject;

namespace RougueBit.Play.DI
{
    public class PlayManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<PlayManager>().AsSingle();
        }
    }
}