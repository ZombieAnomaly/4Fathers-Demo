using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FourFathers
{
    public struct NetworkSessionStruct
    {
        public string RoomName;
        [NonSerialized]
        public SceneRef? Scene;
    }
}
