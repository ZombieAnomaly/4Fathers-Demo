using Fusion;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace FourFathers
{
	public enum ResetType
	{
		UNSET = 0,
		TIMED = 1,
		ON_STOP_INTERACTION = 2,
	}

	public interface IInteractable
	{
		public event Action<IInteractable, bool> InteractionEvent;

		public bool IsInteracted { get; }
		public string InteractionPromptText { get; }
		public bool HoldToInteract { get; }
		public float InteractionHoldTimeSeconds { get; }	
		public TickTimer InteractionTimer { get; }
		public abstract void RPC_ExecuteInteraction(Vector3 interactDirection);
		public abstract void RPC_StartInteraction(Vector3 interactDirection);
		public abstract void RPC_StopInteraction();
	}

	[RequireComponent(typeof(NetworkObject))]
    public abstract class InteractableBase : NetworkBehaviour, IInteractable
	{
		//injected fields
		protected PlayerManager m_playerManager;
		protected InteractableManager m_interactableManager;

		[Inject]
		public void PostInject(PlayerManager playerManager, InteractableManager interactableManager)
		{
            m_playerManager = playerManager;
            m_interactableManager = interactableManager;
		}

		public event Action<IInteractable, bool> InteractionEvent;
		public bool IsInteracted { get { return m_isInteracted; } }
		public bool HoldToInteract { get { return m_holdToInteract; } }
		public float InteractionHoldTimeSeconds { get { return m_interactionHoldTimeSeconds; } }
		public string InteractionPromptText {  get { return m_interactionPromptText; } }
		public TickTimer InteractionTimer => m_interactionTimer;

		[Header("Interactable Settings")]

		[SerializeField]
		protected Collider m_interactableCollider;

		[SerializeField]
		protected string m_interactionPromptText = "Interact";

		[Header("Interactable Toggles")]
		[SerializeField]
		protected bool m_isToggleExectute;

		[SerializeField]
		protected bool m_defaultInteractionState;

		[SerializeField]
		protected bool m_executeOnStart;

		[SerializeField]
		protected bool m_holdToInteract;

		[AllowNesting]
		[ShowIf("m_holdToInteract")]
		[SerializeField]
		protected float m_interactionHoldTimeSeconds;

		[SerializeField]
		protected bool m_resetAfterInteraction;

		[AllowNesting]
		[ShowIf("m_resetAfterInteraction")]
		[SerializeField]
		protected ResetType m_resetType;

		[AllowNesting]
		[ShowIf("ShowResetDelay")]
		[SerializeField]
		protected float m_resetDelaySeconds;

		private bool ShowResetDelay() { return m_resetType == ResetType.TIMED; }

		[Networked]
		protected TickTimer m_interactionTimer { get; set; }

		[Networked]
		protected TickTimer m_resetTimer { get; set; }

		[SerializeField]
		[Networked, OnChangedRender(nameof(OnInteractedChanged))]
		protected bool m_isInteracted { get; set; }

		public abstract void OnInteractedChanged();

		[Networked]
		protected Vector3 m_interactDirection { get; set; }

		public override void Spawned()
        {
			m_isInteracted = m_defaultInteractionState;
			if (m_executeOnStart && m_isInteracted)
				RPC_ExecuteInteraction(m_interactDirection);

			m_interactionTimer = TickTimer.None;
			m_resetTimer = TickTimer.None;

			m_interactableManager.RegisterInteractable(m_interactableCollider, this);
		}

		public override void FixedUpdateNetwork()
		{
			base.FixedUpdateNetwork();

			//if the interaction timer expired execute interaction
			if(m_holdToInteract && m_interactionTimer.Expired(Runner))
			{
				m_interactionTimer = TickTimer.None;
				RPC_ExecuteInteraction(m_interactDirection);
			}

			if(m_resetAfterInteraction && m_resetTimer.Expired(Runner) && m_resetType == ResetType.TIMED)
			{
				m_resetTimer = TickTimer.None;
				ResetInteractable();
			}
		}

		[Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
		public void RPC_StartInteraction(Vector3 interactDirection)
		{
			if (!HasStateAuthority) return;

			m_interactDirection = interactDirection;
			m_interactionTimer = TickTimer.CreateFromSeconds(Runner, m_interactionHoldTimeSeconds);
		}

		[Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
		public void RPC_StopInteraction()
		{
			if (!HasStateAuthority) return;

			m_interactionTimer = TickTimer.None;

			if(m_resetAfterInteraction && m_resetType == ResetType.ON_STOP_INTERACTION)
			{
				ResetInteractable();
			}
		}

		[Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
		public void RPC_ExecuteInteraction(Vector3 interactDirection)
		{
			if (!HasStateAuthority) return;

			m_isInteracted = m_isToggleExectute ? !m_isInteracted : true;
			m_interactDirection = interactDirection;

			if (m_resetAfterInteraction && m_resetType == ResetType.TIMED)
				m_resetTimer = TickTimer.CreateFromSeconds(Runner, m_resetDelaySeconds);

			InteractionEvent?.Invoke(this, m_isInteracted);
		}
		
		private void ResetInteractable()
		{
			if (!HasStateAuthority) return;

			m_isInteracted = m_defaultInteractionState;
			InteractionEvent?.Invoke(this, m_isInteracted);
			m_interactDirection = Vector3.zero;
		}
	}
}
