using Fusion;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace FourFathers
{
	public enum GameState
	{
		UNSET = 0,
		PREGAME = 1,
		PLAYING = 2,
		MATCH_END = 3,
		SHUTDOWN = 4,
	}

    public class GameLogic : MonoBehaviour
	{
		public GameState GameState
		{
			get { return m_gameState; }
		}

		[SerializeField, EnumFlags]
		private GameState m_gameState;

		[SerializeField]
		private int m_minimumPlayersToStart = 2;

		[Inject]
		private NetworkManager m_networkManager;

		[Inject]
		private PlayerManager m_playerManager;

		[Inject]
		private SignalBus m_signalBus;

		[Inject]
		private MatchStartSignal m_matchStartSignal;

		[Inject]
		public void OnInject()
		{
		}

		public void Start()
		{
			SubscribeToNetworkCallbacks();
		}

		public void StartMatch()
		{
			if (!m_networkManager.NetworkRunner.IsServer || 
				m_gameState != GameState.PREGAME ||
				m_playerManager.GetPlayerCount() < m_minimumPlayersToStart) return;

			m_matchStartSignal.StartTime = DateTime.Now;
			m_signalBus.AbstractFire(m_matchStartSignal);
			SetGameState(GameState.PLAYING);
		}

		private void SubscribeToNetworkCallbacks()
		{
			if (m_networkManager.NetworkRunnerCallbackListener == null) return;

			m_networkManager.NetworkRunnerCallbackListener.OnSceneLoadDoneEvent += OnSceneLoaded;

		}

		private void OnSceneLoaded(NetworkRunner runner)
		{
			SetGameState(GameState.PREGAME);
			Debug.Log(string.Format("GameLogic - Scene Loaded! {0}", m_gameState));
			//TO-DO. SEND SIGNAL
		}


		private void SetGameState(GameState gameState)
		{
			m_gameState = gameState;
		}
	}
}
