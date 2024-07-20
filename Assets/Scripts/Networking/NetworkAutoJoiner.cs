using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace FourFathers
{
    public class NetworkAutoJoiner : IInitializable
    {
        [Inject]
        private NetworkManager m_networkManager;

        private NetworkSessionStruct m_sessionParams;
        private GameMode m_gameMode;

        public NetworkAutoJoiner()
        {
           m_sessionParams = NetworkSettings.GetSessionStruct();
        }

        public void Initialize()
        {
            if(m_networkManager.NetworkStatus != NetworkStatus.Disconnected)
            {
                return;
            }

            if(NetworkSettings.IsSinglePlayer)
            {
                m_networkManager.CreateSinglePlayerSession(m_sessionParams);
            }
            else if(NetworkSettings.IsServerBuild)
            {
                m_networkManager.CreateServerSession(m_sessionParams);
            }
            else
            {
                m_networkManager.JoinOrCreateSession(m_sessionParams);
            }
        }
	}
}
