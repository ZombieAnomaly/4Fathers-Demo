using FourFathers;
using UnityEngine;
using Zenject;
using Zenject.SpaceFighter;

public class SceneSignalInstaller : MonoInstaller
{
	public override void InstallBindings()
	{
		Container.BindInterfacesAndSelfTo<MatchStartSignal>().AsTransient();
		Container.DeclareSignalWithInterfaces<MatchStartSignal>();
		Container.BindSignal<MatchStartSignal>()
			.ToMethod<IMatchStartSignalListener>((x, s) => x.OnMatchStart(s))
			.FromResolveAll();

	}
}