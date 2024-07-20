using FourFathers;
using Fusion;
using UnityEngine;
using Zenject;

public class PlayerInstaller : MonoInstaller
{
    [SerializeField]
    private PlayerController m_playerController;

    public override void InstallBindings()
    {
        Container.Bind<PlayerController>()
            .FromInstance(m_playerController)
            .AsSingle()
            .NonLazy();

        Container.Bind<PlayerRef>()
            .FromResolveGetter<PlayerController>(x => x.PlayerRef)
            .AsSingle();

    }
}