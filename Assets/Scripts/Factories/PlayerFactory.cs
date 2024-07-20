using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace FourFathers
{
    public class PlayerFactory : NetworkedFactory<PlayerController>
    {
		private PlayerController m_playerController;

		public PlayerFactory(
			DiContainer container,
			NetworkManager networkManager,
			[Inject(Optional = true)] Transform parentTransform,
			PlayerController playerController) 
			: base(container, networkManager, parentTransform)
		{
			m_playerController = playerController;
		}

		public PlayerController Create(
			Vector3? position = null,
			Quaternion? rotation = null,
			PlayerRef? inputAuthority = null)
		{
			return Create(m_playerController, position, rotation, inputAuthority, beforeSpawnCallback: OnBeforeSpawned);
		}

		private void OnBeforeSpawned(NetworkRunner runner, NetworkObject obj, PlayerController playerController, PlayerRef? inputAuthority)
		{
			if (inputAuthority.HasValue)
			{
				obj.AssignInputAuthority(inputAuthority.Value);
				runner.SetPlayerObject(inputAuthority.Value, obj);
			}
		}

	}
}
