using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace FourFathers
{
	public class EnemyManager : MonoBehaviour, IMatchStartSignalListener
	{

		private AgentBehaviorFactory m_agentFactory;
		private NetworkManager m_networkManager;
		private UIManager m_uiManager;

		[SerializeField]
		private AgentBehavior m_enemyPrototypePrefab;

		[SerializeField, ReadOnly]
		private SerializableDictionary<string, AgentBehavior> m_enemies = new SerializableDictionary<string, AgentBehavior>();

		[Inject]
		public void PostInject(NetworkManager networkManager, UIManager uIManager, AgentBehaviorFactory agentFactory)
		{
			m_networkManager = networkManager;
			m_agentFactory = agentFactory;
			m_uiManager = uIManager;
		}

		public void SpawnEnemy(int amount)
		{
			for(int i=0; i< amount; i++) {
				AgentBehavior agent = m_agentFactory.Create(m_enemyPrototypePrefab, Vector3.up, Quaternion.identity);

				string id = Guid.NewGuid().ToString();
				while (m_enemies.ContainsKey(id))
				{
					id = Guid.NewGuid().ToString();
				}

				agent.ID = id;
				m_enemies.Add(id, agent);
			}
		}

		public void OnMatchStart(MatchStartSignal signal)
		{
			Debug.Log("Match Started! Spawne enemies " + signal.StartTime + "!");
			//SpawnEnemy(1);
		}
	}
}
