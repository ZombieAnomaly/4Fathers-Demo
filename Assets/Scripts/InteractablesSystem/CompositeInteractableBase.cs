using Fusion;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace FourFathers
{
	[Serializable]
	public struct ExpectedInteractableResult
	{
		public InteractableBase Interactable;
		public bool ExpectedInteractionState;
	}

	[RequireComponent(typeof(NetworkObject))]
	public abstract class CompositeInteractableBase : NetworkBehaviour, IInteractable
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
		public string InteractionPromptText => null;
		public TickTimer InteractionTimer => m_interactionTimer;

		[Header("Interactable Settings")]
		[SerializeField]
		protected List<ExpectedInteractableResult> m_expectedInteractableResults = new List<ExpectedInteractableResult>();

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
		protected float m_resetDelaySeconds;

		[Networked]
		protected TickTimer m_interactionTimer { get; set; }

		[Networked]
		protected TickTimer m_resetTimer { get; set; }

		[Networked, OnChangedRender(nameof(OnInteractedChanged))]
		protected bool m_isInteracted { get; set; }

		protected Dictionary<IInteractable, bool> m_currentInteractableResults = new Dictionary<IInteractable, bool>();

		protected Vector3? m_interactDirection;

		public abstract void OnInteractedChanged();

		public void Awake()
		{
			foreach(ExpectedInteractableResult result in m_expectedInteractableResults)
			{
				m_currentInteractableResults[result.Interactable] = false;
				result.Interactable.InteractionEvent += OnExpectedInteractableChange;
			}
		}

		public override void Spawned()
		{
			m_interactionTimer = TickTimer.None;
			m_resetTimer = TickTimer.None;
		}

		public override void FixedUpdateNetwork()
		{
			base.FixedUpdateNetwork();

			//if the interaction timer expired execute interaction
			if (m_holdToInteract && m_interactionTimer.Expired(Runner))
			{
				m_interactionTimer = TickTimer.None;
				Vector3 interactionDir = m_interactDirection.HasValue ? m_interactDirection.Value : Vector3.zero;
				RPC_ExecuteInteraction(interactionDir);
			}

			if (m_resetAfterInteraction && m_resetTimer.Expired(Runner))
			{
				m_resetTimer = TickTimer.None;
				ResetInteractable();
			}
		}

		[Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
		public void RPC_StartInteraction(Vector3 interactionDirection)
		{
			if (!HasStateAuthority) return;

			m_interactDirection = interactionDirection;
			m_interactionTimer = TickTimer.CreateFromSeconds(Runner, m_interactionHoldTimeSeconds);
		}

		[Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
		public void RPC_StopInteraction()
		{
			if (!HasStateAuthority) return;

			m_interactionTimer = TickTimer.None;
		}

		[Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
		public void RPC_ExecuteInteraction(Vector3 interactionDirection)
		{
			if (!HasStateAuthority) return;

			m_isInteracted = m_isToggleExectute ? !m_isInteracted : true;
			m_interactDirection = interactionDirection;

			if (m_resetAfterInteraction)
				m_resetTimer = TickTimer.CreateFromSeconds(Runner, m_resetDelaySeconds);

			InteractionEvent?.Invoke(this, m_isInteracted);
		}

		private void ResetInteractable()
		{
			if (!HasStateAuthority) return;

			m_isInteracted = m_defaultInteractionState;
			InteractionEvent?.Invoke(this, m_isInteracted);
		}

		private void OnExpectedInteractableChange(IInteractable interactable, bool interacted)
		{
			if (m_currentInteractableResults.ContainsKey(interactable))
			{
				m_currentInteractableResults[interactable] = interacted;
			}

			if (IsCurrentInteractablesEqualToExpected())
			{
				Vector3 interactionDir = m_interactDirection.HasValue ? m_interactDirection.Value : Vector3.zero;
				RPC_ExecuteInteraction(interactionDir);
			}
		}

		private bool IsCurrentInteractablesEqualToExpected()
		{
			foreach (ExpectedInteractableResult result in m_expectedInteractableResults)
			{
				//If the expected interactable isn't tracked or does not equal the epxected result, early exit false.
				if (!m_currentInteractableResults.ContainsKey(result.Interactable) ||
					m_currentInteractableResults[result.Interactable] != result.ExpectedInteractionState)
				{
					return false;
				}
			}
			return true;
		}	
	}
}
