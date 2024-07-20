using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace FourFathers
{
    public class NetworkedFactory<TPrefab> : PlaceholderFactory<TPrefab> where TPrefab : MonoBehaviour
    {
        protected readonly Transform ParentTransform;
        private readonly NetworkManager m_networkManager;
        private DiContainer m_container;

        public NetworkedFactory(
            DiContainer container,
			NetworkManager networkManager,
            [Inject(Optional = true)] Transform parentTransform)
        { 
            m_container = container;
			m_networkManager = networkManager;
            ParentTransform = parentTransform;
        }

        public virtual TPrefab Create(
			TPrefab prefab,
            Vector3? position = null,
            Quaternion? rotation = null,
            PlayerRef? inputAuthority = null,
            Action<NetworkRunner, NetworkObject, TPrefab, PlayerRef?> beforeSpawnCallback = null)
		{
			UnityEngine.Assertions.Assert.IsNotNull(prefab, "Null Prefab!");

			void OnBeforeSpawned(NetworkRunner runner, NetworkObject obj)
			{
				if (ParentTransform != null)
				{
					obj.transform.SetParent(ParentTransform);
				}

                var component = obj.GetComponent<TPrefab>();
				beforeSpawnCallback?.Invoke(runner, obj, component, inputAuthority);
			}

			NetworkObject networkedObj = m_networkManager.NetworkRunner.Spawn(
                prefab.gameObject, 
                position, 
                rotation, 
                inputAuthority,
                onBeforeSpawned: OnBeforeSpawned);

			if (networkedObj != null)
			{
				return networkedObj.GetComponent<TPrefab>();
            }
            else
            {
                Debug.LogError(string.Format("Failed to spawn object {0}", prefab));
            }

			return null;
		}

    }


}
