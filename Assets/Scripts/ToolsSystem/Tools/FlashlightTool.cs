using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FourFathers
{
    public class FlashlightTool : ToolBase
    {
		public override void Spawned()
		{
			base.Spawned();
			m_firstPersonRootGameObject.SetActive(false);
			m_thirdPersonRootGameObject.SetActive(false);
		}

		public override void OnUsedChange()
		{
			if (HasInputAuthority)
			{
				m_firstPersonRootGameObject.SetActive(m_isUsed);
			}
			else
			{
				m_thirdPersonRootGameObject.SetActive(m_isUsed);
			}
		}

    }
}
