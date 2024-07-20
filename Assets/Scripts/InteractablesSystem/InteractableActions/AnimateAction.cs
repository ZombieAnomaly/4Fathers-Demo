using Fusion;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricLights;

namespace FourFathers
{
	public enum AnimateParamType
	{
		Trigger = 0,
		Bool = 1,
		Int = 2,
		Float = 3,
	}

	[Serializable]
	public struct InteractionAnimationSettings
	{
		[SerializeField]
		public Animator Animator;

		[SerializeField]
		public NetworkMecanimAnimator NetworkAnimator;

		[AnimatorParam("Animator")]
		[ShowIf("IsValidAnimator")]
		public string AnimationParam;

		[AllowNesting]
		[ShowIf("ShowInteractionSettings")]
		public AnimateParamType ParamType;

		[AllowNesting]
		[ShowIf("AnimateParamTypeIsTrigger")]
		public bool InputAuthorityPassthrough;

		[AllowNesting]
		[ShowIf("AnimateParamTypeIsBool")]
		public bool BoolValue;

		[AllowNesting]
		[ShowIf("AnimateParamTypeIsIntOrFloat")]
		public float FloatValue;

		private bool IsValidAnimator()
		{
			return Animator != null;
		}

		private bool ShowInteractionSettings()
		{
			return !string.IsNullOrEmpty(AnimationParam);
		}

		private bool AnimateParamTypeIsBool()
		{
			return ParamType == AnimateParamType.Bool;
		}

		private bool AnimateParamTypeIsIntOrFloat()
		{
			return ParamType == AnimateParamType.Int || ParamType == AnimateParamType.Float;
		}

		private bool AnimateParamTypeIsTrigger()
		{
			return ParamType == AnimateParamType.Trigger;
		}
	}

    public class AnimateAction : InteractableActionBase
	{
		[Header("Interacted Animation Settings")]

		[SerializeField]
		private InteractionAnimationSettings m_InteractedSettings;

		[Header("Interaction Reset Animation Settings")]

		[SerializeField]
		private InteractionAnimationSettings m_InteractionResetSettings;

		public override void Execute(bool interacted, Vector3? interactionDirection)
		{
			InteractionAnimationSettings settings = interacted ? m_InteractedSettings : m_InteractionResetSettings;
			ExecuteHelper(interacted ? settings.ParamType : settings.ParamType, interacted);
		}

		private void ExecuteHelper(AnimateParamType type, bool interacted)
		{
			InteractionAnimationSettings settings = interacted ? m_InteractedSettings : m_InteractionResetSettings;

			switch (settings.ParamType)
			{
				case AnimateParamType.Bool:
					settings.Animator.SetBool(m_InteractedSettings.AnimationParam, settings.BoolValue);
				break;

				case AnimateParamType.Float:
					settings.Animator.SetFloat(m_InteractedSettings.AnimationParam, settings.FloatValue);
				break;

				case AnimateParamType.Int:
					int intValue = (int)(settings.FloatValue);
					settings.Animator.SetInteger(m_InteractedSettings.AnimationParam, intValue);
				break;

				case AnimateParamType.Trigger:
					settings.NetworkAnimator.SetTrigger(settings.AnimationParam, settings.InputAuthorityPassthrough);
				break;
			}
		}



	}
}
