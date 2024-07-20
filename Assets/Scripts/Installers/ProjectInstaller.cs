using FourFathers;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
	[SerializeField]
	private GameObject m_sceneManager;

	[SerializeField]
	private GameObject m_networkManager;


	public override void InstallBindings()
    {
		Container.BindInterfacesAndSelfTo<CustomSceneManager>()
			.FromComponentInNewPrefab(m_sceneManager)
			.AsSingle()
			.NonLazy();

		Container.BindInterfacesAndSelfTo<NetworkManager>()
			.FromComponentInNewPrefab(m_networkManager)
			.AsSingle()
			.NonLazy();

	}
}