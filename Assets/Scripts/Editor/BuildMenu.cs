using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FourFathers
{
    public class BuildMenu
    {
        public enum NetworkMode
        {
            Host = 0,
            Client = 1,
            Server = 2,
            Configurable = 3
        }

		private const string BUILD_MENU_PATH = "4Fathers/Build/";
		private const string BUILD_MENU_PATH_WINDOWS = "4Fathers/Build/Windows/";
		private const string BUILD_MENU_PATH_MAC = "4Fathers/Build/Mac/";
		private const string BUILD_MENU_PATH_LINUX = "4Fathers/Build/Linux/";
		private const string CACHED_BUILD_PATH_KEY = "4FATHERS_CACHED_BUILD_PATH";
		private const string BUILD_AUTO_RUN_MENU_PATH = BUILD_MENU_PATH + "Auto-Run Post Build";
		private const string AUTO_RUN_BUILD_KEY = "4FATHERS_AUTO_RUN_BUILD";
		public static bool ShouldAutoRun
		{
			get => EditorPrefs.GetBool(AUTO_RUN_BUILD_KEY);
			set => EditorPrefs.SetBool(AUTO_RUN_BUILD_KEY, value);
		}

		[MenuItem(BUILD_AUTO_RUN_MENU_PATH, priority = 0)]
		private static void ToggleAutoRunPostBuild()
		{
			SetAutoRunPostBuild(!ShouldAutoRun);
		}

		[MenuItem(BUILD_AUTO_RUN_MENU_PATH, validate = true, priority = 0)]
		private static bool ValidateToggleAutoRunPostBuild()
		{
			SetAutoRunPostBuild(ShouldAutoRun);
			return true;
		}

		#region Windows Builds
		[MenuItem(BUILD_MENU_PATH_WINDOWS + "Build Host", priority = 2)]
		private static void BuildHostOnWindows()
		{
			Build(BuildTarget.StandaloneWindows64, NetworkMode.Host, GetCurrentTimestampString());
		}

		[MenuItem(BUILD_MENU_PATH_WINDOWS + "Build Dev. Host")]
		private static void BuildDevelopmentHostOnWindows()
		{
			Build(BuildTarget.StandaloneWindows64, NetworkMode.Host, GetCurrentTimestampString(),
				BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging);
		}

		[MenuItem(BUILD_MENU_PATH_WINDOWS + "Build Client")]
		private static void BuildClientOnWindows()
		{
			Build(BuildTarget.StandaloneWindows64, NetworkMode.Client, GetCurrentTimestampString());
		}

		[MenuItem(BUILD_MENU_PATH_WINDOWS + "Build Host + Client", priority = 1)]
		private static void BuildClientAndHostOnWindows()
		{
			if(TryGetBuildDirectory(out string dir))
			{
				string timestamp = GetCurrentTimestampString();
				Build(BuildTarget.StandaloneWindows64, NetworkMode.Client, timestamp, dir);
				Build(BuildTarget.StandaloneWindows64, NetworkMode.Host, timestamp, dir );
			}
		}

		[MenuItem(BUILD_MENU_PATH_WINDOWS + "Build Server")]
		private static void BuildServerOnWindows()
		{
			Build(BuildTarget.StandaloneWindows64, NetworkMode.Server, GetCurrentTimestampString());
		}
		#endregion

		#region OSX Builds
		[MenuItem(BUILD_MENU_PATH_MAC + "Build Host")]
		private static void BuildHostOnOSX()
		{
			Build(BuildTarget.StandaloneOSX, NetworkMode.Host, GetCurrentTimestampString());
		}

		[MenuItem(BUILD_MENU_PATH_MAC + "Build Client")]
		private static void BuildClientOnOSX()
		{
			Build(BuildTarget.StandaloneOSX, NetworkMode.Client, GetCurrentTimestampString());
		}

		[MenuItem(BUILD_MENU_PATH_MAC + "Build Host + Client")]
		private static void BuildClientAndHostOnOSX()
		{
			if (TryGetBuildDirectory(out string dir))
			{
				string timestamp = GetCurrentTimestampString();
				Build(BuildTarget.StandaloneOSX, NetworkMode.Client, timestamp, dir);
				Build(BuildTarget.StandaloneOSX, NetworkMode.Host, timestamp, dir);
			}
		}

		[MenuItem(BUILD_MENU_PATH_MAC + "Build Server")]
		private static void BuildServerOnOSX()
		{
			Build(BuildTarget.StandaloneOSX, NetworkMode.Server, GetCurrentTimestampString());
		}
		#endregion
		
		#region Linux Builds
		[MenuItem(BUILD_MENU_PATH_LINUX + "Build Host")]
		private static void BuildHostOnLinux()
		{
			Build(BuildTarget.StandaloneLinux64, NetworkMode.Host, GetCurrentTimestampString());
		}

		[MenuItem(BUILD_MENU_PATH_LINUX + "Build Client")]
		private static void BuildClientOnLinux()
		{
			Build(BuildTarget.StandaloneLinux64, NetworkMode.Client, GetCurrentTimestampString());
		}

		[MenuItem(BUILD_MENU_PATH_LINUX + "Build Host + Client")]
		private static void BuildClientAndHostOnLinux()
		{
			if (TryGetBuildDirectory(out string dir))
			{
				string timestamp = GetCurrentTimestampString();
				Build(BuildTarget.StandaloneLinux64, NetworkMode.Client, timestamp, dir);
				Build(BuildTarget.StandaloneLinux64, NetworkMode.Host, timestamp, dir);
			}
		}

		[MenuItem(BUILD_MENU_PATH_LINUX + "Build Server")]
		private static void BuildServerOnLinux()
		{
			Build(BuildTarget.StandaloneLinux64, NetworkMode.Server, GetCurrentTimestampString());
		}

		[MenuItem(BUILD_MENU_PATH_LINUX + "Build Headless Server")]
		private static void BuildServerHeadlessOnLinux()
		{
			Build(BuildTarget.LinuxHeadlessSimulation, NetworkMode.Server, GetCurrentTimestampString());
		}
		#endregion

		private static void SetAutoRunPostBuild(bool toggle)
		{
			ShouldAutoRun = toggle;
			Menu.SetChecked(BUILD_AUTO_RUN_MENU_PATH, ShouldAutoRun);
		}

		public static void Build(
			BuildTarget target,
			NetworkMode networkMode,
			string timestamp,
			BuildOptions options = BuildOptions.None)
		{
			if(TryGetBuildDirectory(out string dir))
				Build(target, networkMode, timestamp, dir, options);
		}

		public static void Build(
			BuildTarget target,
			NetworkMode networkMode,
			string timestamp,
			string directory,
			BuildOptions options = BuildOptions.None)
		{
			if (ShouldAutoRun)
			{
				options |= BuildOptions.AutoRunPlayer;
			}

			BuildPlayerOptions buildOptions = new BuildPlayerOptions()
			{
				target = target,
				options = options,
				scenes = GetBuildScenes().ToArray()
			};
			
			switch(networkMode)
			{
				case NetworkMode.Host:
					buildOptions.extraScriptingDefines = new string[] {NetworkSettings.AllowHostModeScriptingDefine};
				break;

				case NetworkMode.Server:
					buildOptions.extraScriptingDefines = new string[] {NetworkSettings.ServerBuildScriptingDefine};
				break;

				case NetworkMode.Configurable:
					buildOptions.extraScriptingDefines = new string[] {NetworkSettings.ConfigurableNetworkModeScriptingDefine};
				break;
			}

			string buildName = Path.ChangeExtension( "4Fathers", GetPlatformExtension(target) );

			buildOptions.locationPathName = Path.Combine(
				directory,
				GetBuildDirectory(target, networkMode, timestamp),
				buildName 
			);

			BuildPipeline.BuildPlayer(buildOptions);
		}

		private static bool TryGetBuildDirectory(out string directory)
		{
			string lastBuildDir = EditorPrefs.GetString(CACHED_BUILD_PATH_KEY);
			directory = EditorUtility.SaveFolderPanel("Select Build Destination", lastBuildDir, "");

			if(string.IsNullOrEmpty(directory))
			{
				return false;
			}

			if(lastBuildDir != directory)
			{
				EditorPrefs.SetString(CACHED_BUILD_PATH_KEY, directory);
			}

			return true;
		}

		private static string GetCurrentTimestampString()
		{
			return DateTime.Now.ToString("yyyy.M.d.HH.mm");
		}

		private static string GetBuildDirectory( BuildTarget target, NetworkMode networkMode, string timestamp)
		{
			string platform = GetPlatformName(target);
			return string.Format("4Fathers_{0}_{1}_{2}", platform, networkMode, timestamp);
		}

		private static string GetPlatformName( BuildTarget target ) 
		{
			switch(target)
			{
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					return "Windows";

				case BuildTarget.StandaloneOSX:
					return "Mac";

				case BuildTarget.EmbeddedLinux:
				case BuildTarget.StandaloneLinux64:
				case BuildTarget.LinuxHeadlessSimulation:
					return "Linux";

				default:
					return target.ToString();
			}
		}

		private static string GetPlatformExtension(BuildTarget target)
		{
			switch (target)
			{
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					return ".exe";

				case BuildTarget.StandaloneOSX:
					return ".app";

				case BuildTarget.StandaloneLinux64:
					return ".x64";

				default:
					throw new NotImplementedException( string.Format("Extension not defined for build target! {0}", target) );
			}
		}

		public static List<string> GetBuildScenes()
		{
			List<string> scenes = new List<string>();

			foreach(EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
			{
				if(scene.enabled)
				{
					scenes.Add(scene.path);
				}
			}

			return scenes;
		}
	}
}
