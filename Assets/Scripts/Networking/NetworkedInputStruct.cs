using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FourFathers
{
	public enum EInputButton
	{
		Jump = 0,
		Fire = 1,
		Reload = 2,
		Spray = 3,
		Sprint = 4,
		Suicide = 5,
		Crouch = 6,
		Interact = 7,
	}

	/// <summary>
	/// Input structure sent over network to the server.
	/// </summary>
	public struct NetworkedInputStruct : INetworkInput
	{
		public Vector2 MoveDirection;
		public Vector2 LookRotationDelta;
		public NetworkButtons Buttons;
	}
}
