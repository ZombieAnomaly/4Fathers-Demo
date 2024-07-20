using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FourFathers
{
    public interface ITool 
    {
		public bool IsUsed { get; }

		public bool HoldToUse { get; }

		public TickTimer UseTimer { get; }

		public abstract void StartUse();
		public abstract void StopUse();
		public abstract void Use();


	}
}
