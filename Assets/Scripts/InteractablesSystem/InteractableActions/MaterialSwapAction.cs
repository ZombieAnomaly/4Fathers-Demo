using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricLights;

namespace FourFathers
{
    public class MaterialSwapAction : InteractableActionBase
	{
		[Header("Material Swap Settings")]
		[SerializeField]
		private MeshRenderer m_mesh;

		[SerializeField]
		private int m_materialIndexToSwap;

		[SerializeField]
		private Material m_materialToSwap;

		private Material[] m_originalMaterials;

		public void Awake()
		{
			m_originalMaterials = m_mesh.materials;
		}

		public override void Execute(bool interacted, Vector3? interactionDirection)
		{
			Material[] newMaterials = m_mesh.materials;
			newMaterials[m_materialIndexToSwap] = m_materialToSwap;
			m_mesh.materials = interacted ? newMaterials : m_originalMaterials;
		}
	}
}
