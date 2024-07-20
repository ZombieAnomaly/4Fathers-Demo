using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Zenject;
using Fusion.Sockets;
using System;
using Fusion.Photon.Realtime;
using UnityEngine.Rendering;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using NaughtyAttributes;

namespace FourFathers
{
	[System.Serializable]
	public enum NetworkStatus
	{
		Disconnected = 0,
		Connecting = 1,
		Connected = 2,
		Failed = 3,
	}

	public class NetworkManager : MonoBehaviour, IMatchStartSignalListener
	{
		private CustomSceneManager m_sceneManager;

		[SerializeField]
		private GameObject m_networkRunnerPrefab;

		[SerializeField]
		private string m_lobbyName = "4Fathers-Default";

		[SerializeField]
		private PhotonAppSettings m_appSettings;

		public NetworkRunner NetworkRunner { get; private set; }

		public NetworkRunnerCallbackListener NetworkRunnerCallbackListener { get; private set; }

		[EnumFlags]
		public NetworkStatus NetworkStatus = NetworkStatus.Disconnected;

		public bool IsServer => NetworkRunner != null && NetworkRunner.IsServer;
		public bool IsResim => NetworkRunner != null && NetworkRunner.IsResimulation;

		[Inject]
		public void PostInject(CustomSceneManager sceneManager)
		{
			m_sceneManager = sceneManager;
			InitializeRunner();
		}

		public ShutdownReason LastShutdownReason {get; private set;}

		public void JoinSession(NetworkSessionStruct sessionParams)
		{
			Debug.Log(string.Format("Joining network session! {0}", sessionParams.RoomName));
			StartSession(GameMode.Client, sessionParams);
		}

		public void CreateHostSession(NetworkSessionStruct sessionParams)
		{
			Debug.Log(string.Format("Creating network session as Host! {0}", sessionParams.RoomName));
			StartSession(GameMode.Host, sessionParams);
		}

		public void CreateServerSession(NetworkSessionStruct sessionParams)
		{
			Debug.Log(string.Format("Creating network session as Server! {0}", sessionParams.RoomName));
			StartSession(GameMode.Server, sessionParams);
		}

		public void JoinOrCreateSession(NetworkSessionStruct sessionParams)
		{
			Debug.Log(string.Format("Joining or creating network session! {0}", sessionParams.RoomName));
			StartSession(GameMode.AutoHostOrClient, sessionParams);
		}

		public void CreateSinglePlayerSession(NetworkSessionStruct sessionParams)
		{
			Debug.Log(string.Format("Starting offline session! {0}", sessionParams.RoomName));
			StartSession(GameMode.Single, sessionParams);
		}

		public void JoinSession() => JoinSession(NetworkSettings.GetSessionStruct());

		public void CreateSession(SceneRef? scene = null)
		{
			CreateHostSession(NetworkSettings.GetSessionStruct(scene));
		}

		public void JoinOrCreateSession(SceneRef? scene = null)
		{
			JoinOrCreateSession(NetworkSettings.GetSessionStruct(scene));
		}

		public void CreateSinglePlayerSession(SceneRef? scene = null)
		{
			CreateSinglePlayerSession(NetworkSettings.GetSessionStruct(scene));
		}

		public void Disconnect()
		{
			if (NetworkRunner == null)
				return;

			SetNetworkStatus(NetworkStatus.Disconnected);
			NetworkRunner.Shutdown();
		}

		private async void StartSession(
			GameMode mode,
			NetworkSessionStruct sessionParams)
		{
			SetNetworkStatus(NetworkStatus.Connecting);
			InitializeRunner();

			NetworkRunner.ProvideInput = mode == GameMode.Server ? false : true;
			FusionAppSettings settings = GetFusionAppSettings();

			Debug.Log(string.Format("Starting multiplayer session {0} ", sessionParams.RoomName));

			StartGameResult results = await NetworkRunner.StartGame(new StartGameArgs
			{
				GameMode = mode,
				SceneManager = m_sceneManager,
				Scene = sessionParams.Scene != null ? sessionParams.Scene : SceneRef.FromIndex( UnitySceneManager.GetActiveScene().buildIndex),
				SessionName = sessionParams.RoomName,
				CustomPhotonAppSettings = settings,
				EnableClientSessionCreation = mode == GameMode.AutoHostOrClient || mode == GameMode.Host ? true : false,
			});


			if(results.Ok == false)
			{
				LastShutdownReason = results.ShutdownReason;
				Debug.LogError(string.Format("Session failed to start! {0}", LastShutdownReason));
				SetNetworkStatus(NetworkStatus.Failed);
			}

		}

		private void SetNetworkStatus(NetworkStatus status)
		{
			if(status == NetworkStatus)
			{
				return;
			}

			NetworkStatus = status;
			//TODO - Fire Signal
		}

		private void InitializeRunner()
		{
			if(NetworkRunner == null)
			{
				var runnerObj = Instantiate(m_networkRunnerPrefab);
				DontDestroyOnLoad(runnerObj);
				NetworkRunner = runnerObj.GetComponent<NetworkRunner>();
				NetworkRunnerCallbackListener = runnerObj.GetComponent<NetworkRunnerCallbackListener>();
				SubscribeToNetworkCallbacks();
			}
		}

		private FusionAppSettings GetFusionAppSettings()
		{
			FusionAppSettings settings = m_appSettings.AppSettings;

			return settings;
		}

		private void SubscribeToNetworkCallbacks()
		{
			NetworkRunnerCallbackListener.OnConnectedToServerEvent += OnConnectedToServer;
			NetworkRunnerCallbackListener.OnConnectFailedEvent += OnConnectFailed;
			NetworkRunnerCallbackListener.OnDisconnectedFromServerEvent += OnDisconnectedFromServer;
			NetworkRunnerCallbackListener.OnShutdownEvent += OnShutdown;
			NetworkRunnerCallbackListener.OnConnectRequestEvent += OnConnectRequest;
			NetworkRunnerCallbackListener.OnPlayerJoinedEvent += OnPlayerJoined;
		}


		#region NetworkRunner Callbacks
		public void OnConnectedToServer(NetworkRunner runner)
		{
			Debug.Log("Connected to server!");
			SetNetworkStatus(NetworkStatus.Connected);
		}

		public void OnConnectFailed(NetworkRunner runner, NetConnectFailedReason reason)
		{
			Debug.Log(string.Format("Connected Failed! {0}", reason));
			SetNetworkStatus(NetworkStatus.Failed);
			Disconnect();
			SetNetworkStatus(NetworkStatus.Disconnected);
		}

		public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
		{
			Debug.Log("Disconnected from server!");
			Disconnect();
			SetNetworkStatus(NetworkStatus.Disconnected);
		}

		public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
		{
			LastShutdownReason = shutdownReason;
			Debug.Log(string.Format("Network Shutdown! {0}", LastShutdownReason));
			SetNetworkStatus(NetworkStatus.Disconnected);

			if(NetworkRunner != null)
			{
				Destroy(NetworkRunner.gameObject);
				NetworkRunner = null;
			}
		}

		public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request)
		{
			request.Accept();
		}

		public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
		{
			SetNetworkStatus(NetworkStatus.Connected);
		}

		public void OnMatchStart(MatchStartSignal signal)
		{
			Debug.Log("[Network Manager] Match Started!");
		}

		#endregion

	}
}
