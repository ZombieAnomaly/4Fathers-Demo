using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;

namespace FourFathers
{

	public class IsPlayerAlive : Conditional
	{
		public SharedGameObject target;

		public override TaskStatus OnUpdate()
		{
			PlayerController playerController = target.Value.GetComponent<PlayerController>();
			if(!playerController)
				playerController = target.Value.GetComponentInParent<PlayerController>();


			if (playerController != null && playerController.IsAlive())
			{
				return TaskStatus.Success;
			}

			return TaskStatus.Failure;
		}
	}
}