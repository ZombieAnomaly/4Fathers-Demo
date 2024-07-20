using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace FourFathers
{
    public class NetworkRunnerFactory : PlaceholderFactory<NetworkRunner>
    {
        private INetworkRunnerCallbacks[] m_callbacks;


        public NetworkRunnerFactory([Inject(Optional = true)] List<INetworkRunnerCallbacks> callbacks) 
        { 
            if(callbacks == null)
            {
                m_callbacks = callbacks.ToArray();
            }
        }

        public override NetworkRunner Create()
        {
            NetworkRunner runner = base.Create();

            if(runner != null && m_callbacks != null) 
            {
                runner.AddCallbacks(m_callbacks);
            }

            return runner;
        }
    }
}
