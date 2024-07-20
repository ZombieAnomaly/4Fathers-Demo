using FourFathers;
using UnityEngine;
using Zenject;

public class SceneInstaller : MonoInstaller
{

	[SerializeField]
	private GameObject m_uIManager;
	public override void InstallBindings()
    {
		Container.BindInterfacesAndSelfTo<UIManager>()
			.FromComponentInNewPrefab(m_uIManager)
			.AsSingle()
			.NonLazy();

	}
}