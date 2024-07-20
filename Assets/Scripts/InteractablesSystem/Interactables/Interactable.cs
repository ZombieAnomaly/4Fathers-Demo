using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricLights;

namespace FourFathers
{
    public class Interactable : InteractableBase
	{
		[SerializeField]
		private List<InteractableActionBase> m_actions = new List<InteractableActionBase>();

		public override void Spawned()
		{
			base.Spawned();
		}

		public override void OnInteractedChanged()
		{
			if (!Runner.IsForward)
				return;

			foreach ( InteractableAction action in m_actions )
			{
				action.Execute(m_isInteracted, m_interactDirection);
			}
		}

	}
}
