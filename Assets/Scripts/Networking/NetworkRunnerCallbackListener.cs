using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FourFathers
{
	public class NetworkRunnerCallbackListener : MonoBehaviour, INetworkRunnerCallbacks
	{
		public event Action<NetworkRunner> OnConnectedToServerEvent;
		public event Action<NetworkRunner, NetConnectFailedReason> OnConnectFailedEvent;
		public event Action<NetworkRunner, NetworkRunnerCallbackArgs.ConnectRequest> OnConnectRequestEvent;
		public event Action<NetworkRunner, NetDisconnectReason> OnDisconnectedFromServerEvent;
		public event Action<NetworkRunner, PlayerRef> OnPlayerJoinedEvent;
		public event Action<NetworkRunner, PlayerRef> OnPlayerLeftEvent;
		public event Action<NetworkRunner> OnSceneLoadDoneEvent;
		public event Action<NetworkRunner> OnSceneLoadStartEvent;
		public event Action<NetworkRunner, ShutdownReason> OnShutdownEvent;


		public void OnConnectedToServer(NetworkRunner runner)
		{
			OnConnectedToServerEvent?.Invoke(runner);
		}

		public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
		{
			OnConnectFailedEvent?.Invoke(runner, reason);
		}

		public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
		{
			OnConnectRequestEvent?.Invoke(runner, request);
		}

		public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
		{
			OnDisconnectedFromServerEvent?.Invoke(runner, reason);
		}

		public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
		{
			OnPlayerJoinedEvent?.Invoke(runner, player);
		}

		public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
		{
			OnPlayerLeftEvent?.Invoke(runner, player);
		}

		public void OnSceneLoadDone(NetworkRunner runner)
		{
			OnSceneLoadDoneEvent?.Invoke(runner);
		}

		public void OnSceneLoadStart(NetworkRunner runner)
		{
			OnSceneLoadStartEvent?.Invoke(runner);
		}

		public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
		{
			OnShutdownEvent?.Invoke(runner, shutdownReason);
		}

		public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
		{
		}

		public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
		{
		}


		public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
		{
		}

		public void OnInput(NetworkRunner runner, NetworkInput input)
		{
		}

		public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
		{
		}

		public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
		{
		}

		public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
		{
		}

		public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
		{
		}

		public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
		{
		}

		public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
		{
		}
	}
}
