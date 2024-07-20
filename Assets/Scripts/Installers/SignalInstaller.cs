using FourFathers;
using UnityEngine;
using Zenject;
using Zenject.SpaceFighter;

public class SignalInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
		SignalBusInstaller.Install(Container);

	}
}