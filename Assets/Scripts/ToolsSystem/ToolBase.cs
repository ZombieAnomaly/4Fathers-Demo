using Fusion;
using NaughtyAttributes;
using UnityEngine;

namespace FourFathers
{
	public abstract class ToolBase : NetworkBehaviour, ITool
	{

		public bool IsUsed { get { return m_isUsed; } }	
		public bool HoldToUse { get { return m_holdToUse; } }
		public TickTimer UseTimer { get { return m_useTimer; } }

		[Networked, OnChangedRender(nameof(OnUsedChange))]
		protected bool m_isUsed { get; set; }

		protected TickTimer m_useTimer { get; set; }

		[Header("Tool Settings")]
		[SerializeField]
		protected bool m_isToggleUse;

		[SerializeField]
		protected bool m_holdToUse;

		[AllowNesting]
		[ShowIf("m_holdToUse")]
		[SerializeField]
		protected float m_useHoldTimeSeconds;

		[SerializeField]
		protected GameObject m_firstPersonRootGameObject;

		[SerializeField]
		protected GameObject m_thirdPersonRootGameObject;

		public abstract void OnUsedChange();

		public override void FixedUpdateNetwork()
		{
			base.FixedUpdateNetwork();

			//if the interaction timer expired execute interaction
			if (m_holdToUse && m_useTimer.Expired(Runner))
			{
				m_useTimer = TickTimer.None;
				Use();
			}
		}

		public void Use()
		{
			if (!HasStateAuthority || !Runner.IsForward) return;

			if (m_isToggleUse)
			{
				m_isUsed = !m_isUsed;
			}
			else
			{
				m_isUsed = true;
			}		
		}

		public void StartUse()
		{
			if (!HasStateAuthority || !Runner.IsForward) return;

			m_useTimer = TickTimer.CreateFromSeconds(Runner, m_useHoldTimeSeconds);
		}

		public void StopUse()
		{
			if (!HasStateAuthority || !Runner.IsForward) return;

			m_useTimer = TickTimer.None;
		}
	}
}
