using FourFathers;
using UnityEngine;
using Zenject;

public class MainMenuInstaller : MonoInstaller
{
    [SerializeField]
    private MainMenuManager m_mainMenuManagerPrefab;

    public override void InstallBindings()
    {

		Container.BindInterfacesAndSelfTo<MainMenuManager>()
			.FromComponentInNewPrefab(m_mainMenuManagerPrefab)
			.UnderTransform(GameObject.Find("Canvas").transform)
			.AsSingle()
			.NonLazy();

	}
}