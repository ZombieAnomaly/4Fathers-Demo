using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FourFathers
{
    public class DoorAction : InteractableActionBase
	{
		[Header("Settings")]
		[SerializeField]
		private bool m_openRelativeToInteraction;

		[SerializeField]
		[ShowIf("m_openRelativeToInteraction")]
		private Transform m_doorTransform;

		[SerializeField]
		private List<InteractableActionBase> m_actions;

		[SerializeField]
		[ShowIf("m_openRelativeToInteraction")]
		private List<InteractableActionBase> m_altActions;


		public override void Execute(bool interacted, Vector3? interactionDirection)
		{
			if(m_openRelativeToInteraction && interactionDirection.HasValue)
			{
				float dot = Vector3.Dot(interactionDirection.Value, m_doorTransform.forward);

				//if player is facing same direction as door forward, open forward
				if(dot > 0) 
				{
					ExecuteActions(interacted, interactionDirection);
				}
				else
				{
					ExecuteAltActions(interacted, interactionDirection);
				}

				return;
			}

			ExecuteActions(interacted, null);
		}

		private void ExecuteActions(bool interacted, Vector3? interactionDirection)
		{
			foreach (InteractableActionBase action in m_actions)
			{
				action.Execute(interacted, interactionDirection.Value);
			}
		}

		private void ExecuteAltActions(bool interacted, Vector3? interactionDirection)
		{
			foreach (InteractableActionBase action in m_altActions)
			{
				action.Execute(interacted, interactionDirection.Value);
			}
		}
	}
}
