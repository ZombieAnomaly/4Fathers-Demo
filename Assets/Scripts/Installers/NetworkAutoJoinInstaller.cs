using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace FourFathers
{
	[CreateAssetMenu(menuName = "4Fathers/Installers/Network Auto Join")]
    public class NetworkAutoJoinInstaller : ScriptableObjectInstaller
    {
		[SerializeField]
		private bool m_shouldAutoJoinSession;

		public override void InstallBindings()
		{
			if (m_shouldAutoJoinSession)
			{
				Container.BindInterfacesAndSelfTo<NetworkAutoJoiner>()
					.AsSingle()
					.NonLazy();
			}
		}
	}
}
