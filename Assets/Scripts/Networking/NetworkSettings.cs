using Fusion;
using UnityEditor;
using UnityEngine;

namespace FourFathers
{
    public static class NetworkSettings
    {
		private const string SINGLE_PLAYER_ENABLED_KEY = "4FATHERS_SIGNLE_PLAYER_ENABLED";
		private const string SERVER_BUILD_ENABLED_KEY = "4FATHERS_SERVER_BUILD_ENABLED";
		private const string GENERAL_NETWORK_SESSION_OVERRIDE_KEY = "4FATHERS_GENERAL_NETWORK_SESSION_OVERRIDE";

		private static NetworkSessionStruct m_overrideParams;


		public static readonly string AllowHostModeScriptingDefine = "4FATHERS_NETWORK_HOSTING_ALLOWED";
		public static readonly string ServerBuildScriptingDefine = "FOUR_FATHERS_SERVER_BUILD";
		public static readonly string ConfigurableNetworkModeScriptingDefine = "4FATHERS_NETWORK_MODE_CONFIGURABLE";

		public static bool IsSinglePlayer 
		{
			get
			{
				#if UNITY_EDITOR
					return EditorPrefs.GetBool(SINGLE_PLAYER_ENABLED_KEY, false);
				#else
					return false;
				#endif
			}

			set
			{
				#if UNITY_EDITOR
					EditorPrefs.SetBool(SINGLE_PLAYER_ENABLED_KEY, value);
				#else
					Debug.Log("Single Player Mode can only be enabled from the Editor!");
				#endif
			}
		}

		public static bool IsServerBuild 
		{
			get
			{
				#if FOUR_FATHERS_SERVER_BUILD
					return true;
				#else
					return false;
				#endif
			}
		}

		public static NetworkSessionStruct GeneralNetworkSessionParams;
		public static NetworkSessionStruct OverrideNetworkSessionParams
		{
			get
			{
				#if UNITY_EDITOR
					string serializedSession = EditorPrefs.GetString(GENERAL_NETWORK_SESSION_OVERRIDE_KEY);
					try
					{
						return JsonUtility.FromJson<NetworkSessionStruct>(serializedSession);
					}
					catch
					{
						return default;
					}
				#else
					return m_overrideParams;
				#endif
			}
			set
			{
				#if UNITY_EDITOR
					string serializedSession = JsonUtility.ToJson(value);
				EditorPrefs.SetString(GENERAL_NETWORK_SESSION_OVERRIDE_KEY, serializedSession);
				#else
					m_overrideParams = value;
				#endif
			}
		}



		public static NetworkSessionStruct GetSessionStruct(SceneRef? scene = null)
		{
			NetworkSessionStruct overrideParams = OverrideNetworkSessionParams;
			//override params are valid
			if (!string.IsNullOrEmpty(overrideParams.RoomName))
			{
				//scene is valid
				if (scene != null)
					overrideParams.Scene = scene;

				return overrideParams;
			}

			if(scene != null)
				GeneralNetworkSessionParams.Scene = scene;

			return GeneralNetworkSessionParams;
		}
	}
}
