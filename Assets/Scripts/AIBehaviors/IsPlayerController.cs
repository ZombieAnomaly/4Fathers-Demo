using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;

namespace FourFathers
{

	public class IsPlayerController : Conditional
	{
		public SharedGameObject target;

		public override TaskStatus OnUpdate()
		{
			PlayerController playerController = target.Value.GetComponent<PlayerController>();
			if (!playerController)
				playerController = target.Value.GetComponentInParent<PlayerController>();

			return playerController != null ? TaskStatus.Success : TaskStatus.Failure;
		}
	}
}