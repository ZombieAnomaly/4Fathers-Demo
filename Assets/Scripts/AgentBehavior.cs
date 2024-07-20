using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static Fusion.NetworkBehaviour;
using BehaviorDesigner.Runtime;
using UnityEngine.AI;
using System;
using Zenject;

namespace FourFathers
{
    public class AgentBehavior : NetworkBehaviour
	{
		[SerializeField, ReadOnly]
		public string ID;

		[SerializeField]
		private BehaviorTree m_behaviorTree;

		[SerializeField]
		private NavMeshAgent m_navmeshAgent;

		[SerializeField]
		private List<TriggerListener> m_attackHitboxTriggerListeners;

		[Inject]
		private PlayerManager m_playerManager;

		private Dictionary<TriggerListener, Dictionary<PlayerController, bool>> m_playersAlreadyHitPerTrigger = new Dictionary<TriggerListener, Dictionary<PlayerController, bool>>();

		public override void Spawned()
		{
			if (!HasStateAuthority)
			{
				m_behaviorTree.enabled = false;
				m_navmeshAgent.enabled = false;
			}

			foreach (TriggerListener trigger in m_attackHitboxTriggerListeners)
			{
				trigger.OnTriggerEnteredEvent += OnHitboxTriggerEnter;
				trigger.OnEnableEvent += OnHitboxEnabled;
			}
		}

		private void OnHitboxEnabled(TriggerListener triggerListener)
		{

			if (m_playersAlreadyHitPerTrigger.ContainsKey(triggerListener))
			{
				m_playersAlreadyHitPerTrigger[triggerListener].Clear();
			}
		}

		private void OnHitboxTriggerEnter(Collider other, TriggerListener triggerListener)
		{
			if (!HasStateAuthority)
				return;

			PlayerController playerController = other.GetComponentInParent<PlayerController>();
			if (!playerController)
				return;

			bool playerAlreadyHit = m_playersAlreadyHitPerTrigger.ContainsKey(triggerListener) && m_playersAlreadyHitPerTrigger[triggerListener].ContainsKey(playerController);
			if (playerAlreadyHit)
				return;

			Debug.Log("[Server] Agent " + name + " Hitbox Event: Collision Enter - " + other.name);

			if(!m_playersAlreadyHitPerTrigger.ContainsKey(triggerListener))
				m_playersAlreadyHitPerTrigger[triggerListener] = new Dictionary<PlayerController, bool>();

			m_playersAlreadyHitPerTrigger[triggerListener][playerController] = true;
			m_playerManager.DownPlayer(playerController.PlayerRef);
		}


	}
}
