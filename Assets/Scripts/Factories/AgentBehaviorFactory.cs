using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace FourFathers
{
	public class AgentBehaviorFactory : NetworkedFactory<AgentBehavior>
	{
		public AgentBehaviorFactory(
			DiContainer container,
			NetworkManager networkManager,
			[Inject(Optional = true)] Transform parentTransform)
			: base(container, networkManager, parentTransform)
		{
		}

		public AgentBehavior Create(
			AgentBehavior agentBehavior,
			Vector3? position = null,
			Quaternion? rotation = null,
			PlayerRef? inputAuthority = null)
		{
			return Create(agentBehavior, position, rotation, inputAuthority, beforeSpawnCallback: OnBeforeSpawned);
		}

		private void OnBeforeSpawned(NetworkRunner runner, NetworkObject obj, AgentBehavior agentBehavior, PlayerRef? inputAuthority)
		{

		}

	}
}
