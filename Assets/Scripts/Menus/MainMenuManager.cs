using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FourFathers
{
	using System.Threading.Tasks;
	using Text = TMPro.TMP_Text;
	using InputField = TMPro.TMP_InputField;
	using UnityEngine;
	using TMPro;
	using global::Fusion.Menu;
	using global::Fusion;
	using Zenject;

	/// <summary>
	/// The main menu.
	/// </summary>
	public class MainMenuManager : MonoBehaviour 
	{
		/// <summary>
		/// The username label.
		/// </summary>
		[InlineHelp, SerializeField] protected Text _usernameLabel;
		/// <summary>
		/// The scene thumbnail. Can be null.
		/// </summary>
		[InlineHelp, SerializeField] protected UnityEngine.UI.Image _sceneThumbnail;
		/// <summary>
		/// The username input UI part.
		/// </summary>
		[InlineHelp, SerializeField] protected GameObject _usernameView;
		/// <summary>
		/// The actual username input field.
		/// </summary>
		[InlineHelp, SerializeField] protected InputField _usernameInput;
		/// <summary>
		/// The username confirmation button (background).
		/// </summary>
		[InlineHelp, SerializeField] protected UnityEngine.UI.Button _usernameConfirmButton;
		/// <summary>
		/// The username change button.
		/// </summary>
		[InlineHelp, SerializeField] protected UnityEngine.UI.Button _usernameButton;
		/// <summary>
		/// The open character selection button.
		/// </summary>
		[InlineHelp, SerializeField] protected UnityEngine.UI.Button _characterButton;
		/// <summary>
		/// The open party screen button.
		/// </summary>
		[InlineHelp, SerializeField] protected UnityEngine.UI.Button _partyButton;
		/// <summary>
		/// The quick play button.
		/// </summary>
		[InlineHelp, SerializeField] protected UnityEngine.UI.Button _playButton;
		/// <summary>
		/// The quit button.
		/// </summary>
		[InlineHelp, SerializeField] protected UnityEngine.UI.Button _quitButton;
		/// <summary>
		/// The open scene screen button.
		/// </summary>
		[InlineHelp, SerializeField] protected UnityEngine.UI.Button _sceneButton;
		/// <summary>
		/// The open setting button.
		/// </summary>
		[InlineHelp, SerializeField] protected UnityEngine.UI.Button _settingsButton;

		[InlineHelp, SerializeField] protected TMP_InputField _roomName;

		[InlineHelp, SerializeField] protected GameObject _roomNamePopup;

		/// <summary>
		/// The Unity awake method. Calls partial method <see cref="AwakeUser"/> to be implemented on the SDK side.
		/// Applies the current selected graphics settings (loaded from PlayerPrefs)
		/// </summary>
		public void Awake()
		{

			new FusionMenuGraphicsSettings().Apply();

			#if UNITY_STANDALONE
						_quitButton.gameObject.SetActive(true);
			#else
				_quitButton.gameObject.SetActive(false);
			#endif
		}

		private NetworkManager m_networkManager;
		//private PlayerManager m_playerManager;
		private NetworkSessionStruct m_sessionParams;

		[Inject]
		public void PostInject(NetworkManager networkManager)
		{
			m_networkManager = networkManager;
		}


		/// <summary>
		/// The screen show method. Calls partial method <see cref="ShowUser"/> to be implemented on the SDK side.
		/// </summary>
		public void Show()
		{

			_roomNamePopup.SetActive(false);
			_usernameView.SetActive(false);
			_usernameLabel.text = "Zombie";


				_sceneButton.interactable = false;
		}

		/// <summary>
		/// Is called when the sceen background is pressed using SendMessage() from the UI object.
		/// </summary>
		protected virtual void OnFinishUsernameEdit()
		{
			OnFinishUsernameEdit(_usernameInput.text);
		}

		/// <summary>
		/// Is called when the <see cref="_usernameInput"/> has finished editing using SendMessage() from the UI object.
		/// </summary>
		protected virtual void OnFinishUsernameEdit(string username)
		{
			_usernameView.SetActive(false);

			if (string.IsNullOrEmpty(username) == false)
			{
				_usernameLabel.text = username;
			}
		}

		/// <summary>
		/// Is called when the <see cref="_usernameButton"/> is pressed using SendMessage() from the UI object.
		/// </summary>
		protected virtual void OnUsernameButtonPressed()
		{
			_usernameView.SetActive(true);
			_usernameInput.text = _usernameLabel.text;
		}

		/// <summary>
		/// Is called when the <see cref="_playButton"/> is pressed using SendMessage() from the UI object.
		/// Intitiates the connection and expects the connection object to set further screen states.
		/// </summary>
		protected virtual void OnQuickPlayButtonPressed()
		{
			m_sessionParams = NetworkSettings.GetSessionStruct(SceneRef.FromIndex(1));
			m_networkManager.JoinOrCreateSession(m_sessionParams);
		}

		/// <summary>
		/// Is called when the <see cref="_playButton"/> is pressed using SendMessage() from the UI object.
		/// Intitiates the connection and expects the connection object to set further screen states.
		/// </summary>
		protected virtual void OnJoinButtonPressed()
		{
			m_sessionParams = NetworkSettings.GetSessionStruct(SceneRef.FromIndex(1));
			m_sessionParams.RoomName = string.IsNullOrEmpty(_roomName.text) ? m_sessionParams.RoomName : _roomName.text;
			m_networkManager.JoinSession(m_sessionParams);
		}

		/// <summary>
		/// Is called when the <see cref="_playButton"/> is pressed using SendMessage() from the UI object.
		/// Intitiates the connection and expects the connection object to set further screen states.
		/// </summary>
		protected void OnHostJoinButtonPressed()
		{
			_roomNamePopup.SetActive(!_roomNamePopup.activeInHierarchy);
		}

		/// <summary>
		/// Is called when the <see cref="_playButton"/> is pressed using SendMessage() from the UI object.
		/// Intitiates the connection and expects the connection object to set further screen states.
		/// </summary>
		protected virtual void OnHostButtonPressed()
		{
			m_sessionParams = NetworkSettings.GetSessionStruct(SceneRef.FromIndex(1));
			m_sessionParams.RoomName = string.IsNullOrEmpty(_roomName.text) ? m_sessionParams.RoomName : _roomName.text;
			m_networkManager.CreateHostSession(m_sessionParams);
		}

		/// <summary>
		/// Default connection error handling is reused in a couple places.
		/// </summary>
		/// <param name="result">Connect result</param>
		/// <param name="controller">UI Controller</param>
		/// <returns>When handling is completed</returns>
		public static async Task HandleConnectionResult(ConnectResult result, IFusionMenuUIController controller)
		{
			if (result.CustomResultHandling == false)
			{
				if (result.Success)
				{
					controller.Show<FusionMenuUIGameplay>();
				}
				else if (result.FailReason != ConnectFailReason.ApplicationQuit)
				{
					var popup = controller.PopupAsync(result.DebugMessage, "Connection Failed");
					if (result.WaitForCleanup != null)
					{
						await Task.WhenAll(result.WaitForCleanup, popup);
					}
					else
					{
						await popup;
					}
					controller.Show<FusionMenuUIMain>();
				}
			}
		}

		/// <summary>
		/// Is called when the <see cref="_partyButton"/> is pressed using SendMessage() from the UI object.
		/// </summary>
		protected virtual void OnPartyButtonPressed()
		{
			//Controller.Show<FusionMenuUIParty>();
		}

		/// <summary>
		/// Is called when the <see cref="_sceneButton"/> is pressed using SendMessage() from the UI object.
		/// </summary>
		protected virtual void OnScenesButtonPressed()
		{
			//Controller.Show<FusionMenuUIScenes>();
		}

		/// <summary>
		/// Is called when the <see cref="_settingsButton"/> is pressed using SendMessage() from the UI object.
		/// </summary>
		protected virtual void OnSettingsButtonPressed()
		{
			//Controller.Show<FusionMenuUISettings>();
		}

		/// <summary>
		/// Is called when the <see cref="_characterButton"/> is pressed using SendMessage() from the UI object.
		/// </summary>
		protected virtual void OnCharacterButtonPressed()
		{
		}

		/// <summary>
		/// Is called when the <see cref="_quitButton"/> is pressed using SendMessage() from the UI object.
		/// </summary>
		protected virtual void OnQuitButtonPressed()
		{
			Application.Quit();
		}
	}

}
