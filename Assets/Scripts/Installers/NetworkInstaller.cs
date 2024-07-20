using FourFathers;
using Fusion;
using UnityEngine;
using VolumetricLightsDemo;
using Zenject;

namespace FourFathers
{
    public class NetworkInstaller : MonoInstaller
    {

	    [SerializeField]
	    private NetworkSessionStruct m_generalNetworkSessionParams = new()
        {
            RoomName = "Default"
        };

		[SerializeField]
		private GameLogic m_gameLogic;

		[SerializeField]
		private PlayerManager m_playerManager;

		[SerializeField]
		private EnemyManager m_enemyManager;

		[SerializeField]
		private InteractableManager m_interactableManager;

		[SerializeField]
		private PlayerController m_playerPrefab;


		public override void InstallBindings()
        {
            NetworkSettings.GeneralNetworkSessionParams = m_generalNetworkSessionParams;

			Container.BindFactory<PlayerController, PlayerFactory>()
				.WithFactoryArguments(m_playerPrefab)
				.AsSingle();

			Container.BindFactory<AgentBehavior, AgentBehaviorFactory>()
				.AsSingle();

			Container.BindInterfacesAndSelfTo<GameLogic>()
				.FromInstance(m_gameLogic)
				.AsSingle();
			Container.QueueForInject(m_gameLogic);

			Container.BindInterfacesAndSelfTo<PlayerManager>()
				.FromInstance(m_playerManager)
				.AsSingle();
			Container.QueueForInject(m_playerManager);

			Container.BindInterfacesAndSelfTo<EnemyManager>()
				.FromInstance(m_enemyManager)
				.AsSingle();
			Container.QueueForInject(m_enemyManager);

			Container.BindInterfacesAndSelfTo<InteractableManager>()
				.FromInstance(m_interactableManager)
				.AsSingle();
			Container.QueueForInject(m_interactableManager);



		}
    }
}