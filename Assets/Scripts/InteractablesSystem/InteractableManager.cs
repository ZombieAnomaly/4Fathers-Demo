using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace FourFathers
{
    public class InteractableManager : MonoBehaviour
    {
		private NetworkManager m_networkManager;
		private UIManager m_uiManager;

		[SerializeField, ReadOnly]
		private SerializableDictionary<Collider, IInteractable> m_interactables = new SerializableDictionary<Collider, IInteractable>();

		[Inject]
		public void PostInject(NetworkManager networkManager, UIManager uIManager)
		{
			m_networkManager = networkManager;
			m_uiManager = uIManager;
		}

		public void RegisterInteractable(Collider collider, IInteractable interactable)
		{
			m_interactables[collider] = interactable;
		}

		public void InteractStart(Collider collider, Vector3? interactedDirection)
		{
			if (!m_interactables.ContainsKey(collider))
			{
				Debug.Log("Collider does not exists in the interactables dictionary. " + collider.name);
				return;
			}

			IInteractable interactable = m_interactables[collider];
			Vector3 interactionDir = interactedDirection.HasValue ? interactedDirection.Value : Vector3.zero;

			if (interactable.HoldToInteract)
			{
				m_uiManager.StartInteractHoldProgress(interactable.InteractionHoldTimeSeconds);
				interactable.RPC_StartInteraction(interactionDir);
			}
			else
			{
				interactable.RPC_ExecuteInteraction(interactionDir);
			}
		}

		public void InteractStop(Collider collider)
		{
			if (!m_interactables.ContainsKey(collider))
			{
				Debug.Log("Collider does not exists in the interactables dictionary. " + collider.name);
				return;
			}

			IInteractable interactable = m_interactables[collider];
			// if this interactable is holdToInteract and the interactionTimer has not expired, stop interact
			if (interactable.HoldToInteract && !interactable.InteractionTimer.Expired(m_networkManager.NetworkRunner))
			{
				interactable.RPC_StopInteraction();
			}

			if(interactable.HoldToInteract)
				m_uiManager.StopInteractHoldProgress();
		}

		public void ShowInteractPrompt(Collider collider)
		{
			if (!m_interactables.ContainsKey(collider))
			{
				Debug.Log("Collider does not exists in the interactables dictionary. " + collider.name);
				return;
			}

			IInteractable interactable = m_interactables[collider];
			m_uiManager.ShowInteractPrompt(interactable.HoldToInteract, interactable.InteractionPromptText);
		}

		public void HideInteractPrompt()
		{
			m_uiManager.HideInteractPrompt();
		}
	}
}
