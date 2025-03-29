using UnityEngine;
using Zenject;

namespace RougueBit.Title.DI
{
    public class TitleManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<ITitleManager>().FromNew().AsSingle();
        }
    }
}
