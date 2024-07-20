using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace FourFathers
{
	public class RevivePlayerAction : InteractableActionBase
	{
		[SerializeField]
		private PlayerController m_targetPlayerController;

		//injected fields
		protected PlayerManager m_playerManager;
		protected InteractableManager m_interactableManager;

		[Inject]
		public void PostInject(PlayerManager playerManager, InteractableManager interactableManager)
		{
			m_playerManager = playerManager;
			m_interactableManager = interactableManager;
		}

		public override void Execute(bool interacted, Vector3? interactionDirection)
		{
			//Debug.Log("Attempting to revive " + m_targetPlayerController.PlayerRef);

			if(interacted && HasStateAuthority)
			{
				m_playerManager.RevivePlayer(m_targetPlayerController.PlayerRef);
			}
		}
	}
}
