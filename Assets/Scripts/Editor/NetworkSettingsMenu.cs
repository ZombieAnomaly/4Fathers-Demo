using UnityEditor;

namespace FourFathers
{
    public static class NetworkSettingsMenu
    {
        private const string SINGLE_PLAYER_MODE_MENU_NAME = "4Fathers/Networking/Single Player Mode";
		private const string NETWORK_SESSION_OVERRIDE_MENU_PATH = "4Fathers/Networking/Editor Session/";
		private const string NETWORK_SESSION_OVERRIDE_GENERAL_MENU_NAME = NETWORK_SESSION_OVERRIDE_MENU_PATH + "General";
		private const string NETWORK_SESSION_OVERRIDE_Kehran_MENU_NAME = NETWORK_SESSION_OVERRIDE_MENU_PATH + "Kehran";

		public static readonly NetworkSessionStruct NETWORK_SESSION_OVERRIDE_KEHRAN = new NetworkSessionStruct()
		{
			RoomName = "Kehran"
		};

		public static readonly NetworkSessionStruct NETWORK_SESSION_OVERRIDE_General = new NetworkSessionStruct()
		{
			RoomName = "4Fathers-Default"
		};


		public static void SetSinglePlayerModeEnabled(bool toggle)
		{
			NetworkSettings.IsSinglePlayer = toggle;
			Menu.SetChecked( SINGLE_PLAYER_MODE_MENU_NAME, toggle );
		}
		
		public static void SetNetworkSessionOverride(NetworkSessionStruct? overrideParams)
		{
			if(overrideParams.HasValue && !string.IsNullOrEmpty(overrideParams.Value.RoomName))
			{
				NetworkSettings.OverrideNetworkSessionParams = overrideParams.Value;
			}
			else
			{
				NetworkSettings.OverrideNetworkSessionParams = default;
			}

			UpdateSessionOverrideMenu();
		}

		private static void UpdateSessionOverrideMenu()
		{
			NetworkSessionStruct overrideParams = NetworkSettings.OverrideNetworkSessionParams;

			if(!string.IsNullOrEmpty(overrideParams.RoomName)) 
			{
				Menu.SetChecked(NETWORK_SESSION_OVERRIDE_GENERAL_MENU_NAME, false);

				Menu.SetChecked(NETWORK_SESSION_OVERRIDE_Kehran_MENU_NAME, overrideParams.RoomName == NETWORK_SESSION_OVERRIDE_KEHRAN.RoomName);
			}
			else
			{
				Menu.SetChecked(NETWORK_SESSION_OVERRIDE_GENERAL_MENU_NAME, true);

				Menu.SetChecked(NETWORK_SESSION_OVERRIDE_Kehran_MENU_NAME, false);
			}
		}

		[MenuItem(NETWORK_SESSION_OVERRIDE_GENERAL_MENU_NAME)]
		private static void EnableGeneralNetworkSessionOverride()
		{
			SetNetworkSessionOverride(NETWORK_SESSION_OVERRIDE_General);
		}

		[MenuItem(NETWORK_SESSION_OVERRIDE_GENERAL_MENU_NAME, validate = true)]
		private static bool ValidateGeneralNetworkSessionOverride()
		{
			UpdateSessionOverrideMenu();
			return true;
		}

		[MenuItem(NETWORK_SESSION_OVERRIDE_Kehran_MENU_NAME)]
		private static void EnableKehranNetworkSessionOverride()
		{
			SetNetworkSessionOverride(NETWORK_SESSION_OVERRIDE_KEHRAN);
		}


		[MenuItem(SINGLE_PLAYER_MODE_MENU_NAME, priority = 0)]
		private static void ToggleSinglePlayer()
		{
			SetSinglePlayerModeEnabled(!NetworkSettings.IsSinglePlayer);
		}


		[MenuItem(SINGLE_PLAYER_MODE_MENU_NAME, priority = 0, validate = true)]
		private static bool ValidateSinglePlayer()
		{
			bool isEnabled = NetworkSettings.IsSinglePlayer;
			SetSinglePlayerModeEnabled(isEnabled);
			return true;
		}
	}
}
