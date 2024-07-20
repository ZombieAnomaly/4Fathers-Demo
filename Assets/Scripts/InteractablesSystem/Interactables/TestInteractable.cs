using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FourFathers
{
	public class TestInteractable : InteractableBase
	{
		public override void Spawned()
		{
			base.Spawned();
		}

		public override void OnInteractedChanged()
		{
			Debug.Log("Test Interaction...");
		}
	}
}
