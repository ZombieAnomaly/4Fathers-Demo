using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FourFathers
{
	
	public class TriggerListener : MonoBehaviour
	{
		public event Action<Collider, TriggerListener> OnTriggerEnteredEvent;
		public event Action<Collider, TriggerListener> OnTriggerExitedEvent;
		public event Action<Collider, TriggerListener> OnTriggerStayEvent;
		public event Action<TriggerListener> OnDisableEvent;
		public event Action<TriggerListener> OnEnableEvent;

		private void OnTriggerEnter(Collider other)
		{
			OnTriggerEnteredEvent?.Invoke(other, this);
		}

		private void OnTriggerExit(Collider other)
		{
			OnTriggerExitedEvent?.Invoke(other, this);
		}

		private void OnTriggerStay(Collider other)
		{
			OnTriggerStayEvent?.Invoke(other, this);
		}

		public void OnEnable()
		{
			OnEnableEvent?.Invoke(this);
		}

		public void OnDisable()
		{
			OnDisableEvent?.Invoke(this);
		}
	}
}
