using UnityEngine;
using Zenject;

namespace RougueBit.Play.DI
{
    [CreateAssetMenu(fileName = "PlaySceneSettingsInstaller", menuName = "Installers/PlaySceneSettingsInstaller")]
    public class PlaySceneSettingsInstaller : ScriptableObjectInstaller<PlaySceneSettingsInstaller>
    {
        [SerializeField] private PlaySceneSO playSceneSO;

        public override void InstallBindings()
        {
            Container.BindInstance(playSceneSO).AsSingle();
        }
    }
}
