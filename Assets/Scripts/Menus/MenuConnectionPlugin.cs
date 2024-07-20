using UnityEngine;
using Fusion.Menu;

namespace FourFathers
{
	public abstract class MenuConnectionPlugin : MonoBehaviour
	{
		public abstract IFusionMenuConnection Create(MenuConnectionBehaviour connectionBehaviour);
	}
}
