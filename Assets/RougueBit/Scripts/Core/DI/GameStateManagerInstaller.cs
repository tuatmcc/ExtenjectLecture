using Zenject;

namespace RougueBit.Core.DI
{
    public class GameStateManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<GameStateManager>().FromNew().AsSingle();
        }
    }
}