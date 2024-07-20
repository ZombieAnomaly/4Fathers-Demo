using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricLights;

namespace FourFathers
{
    
    public class LightAction : InteractableActionBase
    {
		[Header("Light Settings")]
		[SerializeField]
		private Light m_light;

		[SerializeField]
		private VolumetricLight m_volumetricLight;

		public override void Execute(bool interacted, Vector3? interactionDirection)
		{
			m_light.enabled = !m_light.enabled;
			m_volumetricLight.enabled = !m_volumetricLight.enabled;
		}
	}
}
