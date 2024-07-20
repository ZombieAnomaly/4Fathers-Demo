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
	[Serializable]
	public struct PlayerData : INetworkStruct
	{
		public float MaxStamina;
		public float CurrentStamina;
		public int DownCount;

		public TickTimer RespawnTimer;
		public TickTimer DownedTimer;
		public TickTimer StaminaTimer;

		public PlayerData(float maxStamina)
		{
			MaxStamina = maxStamina;
			CurrentStamina = maxStamina;
			DownCount = 0;
			RespawnTimer = TickTimer.None;
			DownedTimer = TickTimer.None;
			StaminaTimer = TickTimer.None;
		}
	}

    public class PlayerManager : NetworkBehaviour
	{

		private PlayerFactory m_playerFactory;
		private NetworkManager m_networkManager;
		private UIManager m_uiManager;
		private GameLogic m_gameLogic;

		[SerializeField]
		private int m_maxDownsBeforeDeath = 1;
		[SerializeField]
		private float m_respawnDelaySeconds = 5f;
		[SerializeField]
		private float m_downedTimeUntilDeath = 15f;
		[SerializeField]
		private float Default_MaxStamina = 100f;
		[SerializeField]
		private float Default_StaminaDrainRatePerSecond = 20f;
		[SerializeField]
		private float Default_StaminaRegenRatePerSecond = 10f;
		[SerializeField]
		private float Default_StaminaRegenDelaySeconds = 3f;

		[SerializeField]
		[Networked, Capacity(6)]
		private NetworkDictionary<PlayerRef, PlayerData> m_players => default;
		
		[SerializeField]
		[Networked, Capacity(6)]
		private NetworkDictionary<PlayerRef, PlayerController> m_playerControllers => default;

		public PlayerController LocalPlayer
        {
			get
			{
				if(m_networkManager.NetworkRunner != null &&
					m_playerControllers.TryGet(m_networkManager.NetworkRunner.LocalPlayer, out PlayerController playerController))
				{
					return playerController;
				}

				return null;
			}
        }

		[Inject]
		public void PostInject(NetworkManager networkManager, UIManager uIManager, PlayerFactory playerFactory, GameLogic gameLogic)
		{
			m_networkManager = networkManager;
			m_playerFactory = playerFactory;
			m_uiManager = uIManager;
			m_gameLogic = gameLogic;

			SubscribeToNetworkCallbacks();
		}

		private void SubscribeToNetworkCallbacks()
		{
			if (m_networkManager.NetworkRunnerCallbackListener == null) return;

			m_networkManager.NetworkRunnerCallbackListener.OnPlayerJoinedEvent += OnPlayerJoined;
			m_networkManager.NetworkRunnerCallbackListener.OnPlayerLeftEvent += OnPlayerLeft;

		}

		public override void FixedUpdateNetwork()
		{
			if (!Runner.IsServer)
				return;

			foreach( KeyValuePair<PlayerRef, PlayerData> kvp in m_players)
			{
				PlayerRef playerRef = kvp.Key;
				PlayerData playerData = kvp.Value;

				TickTimer respawnTimer = playerData.RespawnTimer;
				TickTimer downedTimer = playerData.DownedTimer;
				TickTimer staminaTimer = playerData.StaminaTimer;

				if (respawnTimer.Expired(Runner))
				{
					playerData.RespawnTimer = TickTimer.None;
					m_playerControllers[playerRef].Respawn();
					m_players.Set(playerRef, playerData);
				}

				if (downedTimer.Expired(Runner) && m_playerControllers[playerRef].IsAlive())
				{
					KillPlayer(playerRef);
				}
			}
		}	

		public int GetPlayerCount()
		{
			return m_players.Count;
		}

		public PlayerController GetPlayerController(PlayerRef playerRef)
		{
			return m_playerControllers[playerRef];
		}

		public float? GetPlayerStamina(PlayerRef playerRef)
		{
			if (!m_players.ContainsKey(playerRef))
			{
				Debug.LogError("Unable to find player! " + playerRef.PlayerId);
				return null;
			}
			return m_players.Get(playerRef).CurrentStamina;
		}

		public void RegenerateStamina(PlayerRef playerRef, float dt)
		{
			bool hasPlayerData = m_players.TryGet(playerRef, out PlayerData playerData);

			if (!hasPlayerData || !m_playerControllers[playerRef].HasInputAuthority && !Runner.IsServer)
				return;

			if (!CanPlayerRegenerateStamina(playerRef))
				return;

			ModifyPlayerStamina(playerRef, Default_StaminaRegenRatePerSecond * dt);
		}

		public void DrainPlayerStamina(PlayerRef playerRef, float dt)
		{
			bool hasPlayerData = m_players.TryGet(playerRef, out PlayerData playerData);

			if (!hasPlayerData || !m_playerControllers[playerRef].HasInputAuthority && !Runner.IsServer)
				return;

			ModifyPlayerStamina(playerRef, -Default_StaminaDrainRatePerSecond * dt );
		}

		public void AddPlayer(PlayerRef playerRef, PlayerController playerController)
		{
			if (!Runner.IsServer)
				return;

			m_players.Set(playerRef, new PlayerData(Default_MaxStamina));
			m_playerControllers.Set(playerRef, playerController);

			//A new player has joined, attempt to start the match
			m_gameLogic.StartMatch();
		}

		public void RemovePlayer(PlayerRef playerRef)
		{
			if (!Runner.IsServer)
				return;

			m_players.Remove(playerRef);
			m_playerControllers.Remove(playerRef);
		}

		public void OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef)
		{
			if (Runner.IsServer)
			{
				PlayerController playerController = m_playerFactory.Create(Vector3.up, Quaternion.identity, inputAuthority: playerRef);

				Debug.Log(string.Format("[Server] Spawning Player {0}", playerRef));
			}
		}

		public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
		{
			if (Runner.IsServer && m_playerControllers.TryGet(player, out PlayerController playerController))
			{
				if(playerController != null && playerController.HasStateAuthority)
				{
					m_networkManager.NetworkRunner.Despawn(playerController.Object);
				}
				Debug.Log(string.Format("[Server] Despawning Player {0}", player));
			}

			RemovePlayer(player);
		}

		public void KillPlayer(PlayerRef playerRef)
		{
			if (!Runner.IsServer)
				return;

			m_playerControllers[playerRef].Kill();
			PlayerData playerData = m_players.Get(playerRef);

			playerData.DownCount = 0;
			playerData.DownedTimer = TickTimer.None;

			TickTimer timer = TickTimer.CreateFromSeconds(Runner, m_respawnDelaySeconds);
			playerData.RespawnTimer = timer;

			m_players.Set(playerRef, playerData);
		}

		public void DownPlayer(PlayerRef playerRef)
		{
			if (!Runner.IsServer)
				return;

			PlayerData playerData = m_players.Get(playerRef);
			playerData.DownCount++;

			if (m_players[playerRef].DownCount > m_maxDownsBeforeDeath)
			{
				KillPlayer(playerRef);
				return;
			}

			m_playerControllers[playerRef].Down();

			TickTimer timer = TickTimer.CreateFromSeconds(Runner, m_downedTimeUntilDeath);
			playerData.DownedTimer = timer;

			m_players.Set(playerRef, playerData);
		}

		public void RevivePlayer(PlayerRef playerRef)
		{
			if (!Runner.IsServer)
				return;

			if (!m_playerControllers[playerRef].IsAlive())
				return;

			PlayerData playerData = m_players.Get(playerRef);

			m_playerControllers[playerRef].Revive();
			playerData.DownedTimer = TickTimer.None;

			m_players.Set(playerRef, playerData);
		}

		private bool CanPlayerRegenerateStamina(PlayerRef playerRef)
		{
			bool hasPlayerData = m_players.TryGet(playerRef, out PlayerData playerData);


			return hasPlayerData && playerData.StaminaTimer.Expired(Runner) && playerData.CurrentStamina < playerData.MaxStamina;
		}

		private void ModifyPlayerStamina(PlayerRef playerRef, float amount)
		{
			bool hasPlayerData = m_players.TryGet(playerRef, out PlayerData playerData);

			if (!hasPlayerData)
				return;

			// if player is losing stamina, reset stamina regen delay
			if(amount < 0)
			{
				playerData.StaminaTimer = TickTimer.CreateFromSeconds(Runner, Default_StaminaRegenDelaySeconds);
			}

			playerData.CurrentStamina += amount;
			Mathf.Clamp(playerData.CurrentStamina, 0f, playerData.MaxStamina);
			m_players.Set(playerRef, playerData);
		}
	}
}
