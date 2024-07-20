using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FourFathers
{
	public interface IMatchStartSignal {
		public DateTime StartTime { get; }
	}
	public interface IMatchStartSignalListener
	{
		public void OnMatchStart(MatchStartSignal signal);
	}
	
	public class MatchStartSignal: IMatchStartSignal
	{
        public DateTime StartTime { get; set; }

    }
}
