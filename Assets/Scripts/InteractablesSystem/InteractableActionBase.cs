using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FourFathers
{
	public interface InteractableAction
    {
		public abstract void Execute(bool interacted, Vector3? interactionDirection);
	}

	public class InteractableActionBase : NetworkBehaviour, InteractableAction
	{
		public virtual void Execute(bool interacted, Vector3? interactionDirection)
        {

        }

    }
}
