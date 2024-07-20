using Fusion;
using SimpleFPS;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace FourFathers
{
	public interface IUIManager
	{
		public void ToggleDeathView(bool toggle);
	}

	public class UIManager : MonoBehaviour, IUIManager
    {
		[HideInInspector]
		public NetworkRunner Runner;

		[SerializeField]
		private GameObject m_scoreboardView;
		[SerializeField]
		private GameObject m_menuView;
		[SerializeField]
		private GameObject m_disconnectedView;
		[SerializeField]
		private GameObject m_deathView;
		[SerializeField]
		private GameObject m_crosshair;
		[SerializeField]
		private GameObject m_interactPrompt;
		[SerializeField]
		private TextMeshProUGUI m_interactPromptText;
		[SerializeField]
		private Slider m_interactHoldSlider;

		[Inject]
		private GameLogic GameLogic;

		private DateTime? m_startHoldInteractionTime;
		private DateTime? m_endHoldInteractionTime;
		private TimeSpan? m_interactionTimeSpan;

		// Called from NetworkEvents on NetworkRunner object
		public void OnRunnerShutdown(NetworkRunner runner, ShutdownReason reason)
		{
			m_disconnectedView.SetActive(true);
		}

		public void GoToMenu()
		{
			if (Runner != null){
				Runner.Shutdown();
			}

			//SceneManager.LoadScene("MainMenu");
		}

		public void SetCamera(Camera camera)
		{
			GetComponent<Canvas>().worldCamera = camera;
		}

		private void Awake()
		{
			m_menuView.SetActive(false);
			m_disconnectedView.SetActive(false);
			m_deathView.SetActive(false);
			m_interactPrompt.SetActive(false);

			// Make sure the cursor starts unlocked
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		private void Update()
		{
			if (Application.isBatchMode == true)
				return;

			//if (GameLogic.Object == null || GameLogic.Object.IsValid == false)
			//	return;

			//Runner = GameLogic.Runner;

			var keyboard = Keyboard.current;
			bool gameplayActive = true;//GameLogic.State < EGameplayState.Finished;

			m_scoreboardView.SetActive(gameplayActive && keyboard != null && keyboard.tabKey.isPressed);

			if (gameplayActive && keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
			{
				m_menuView.SetActive(!m_menuView.activeSelf);
			}

			DateTime now = DateTime.UtcNow;
			if(m_endHoldInteractionTime.HasValue && now <= m_endHoldInteractionTime)
			{
				m_interactionTimeSpan = m_endHoldInteractionTime.Value.Subtract(now);
				float secondsLeft = (float)m_interactionTimeSpan.Value.TotalSeconds;
				float totalSeconds = (float)m_endHoldInteractionTime.Value.Subtract(m_startHoldInteractionTime.Value).TotalSeconds;

				float progressValue = Mathf.Clamp(1-(secondsLeft/totalSeconds), 0f, 1f);
				m_interactHoldSlider.value = progressValue;
			}
		}

		public void ToggleDeathView(bool toggle)
		{
			m_deathView.SetActive(toggle);
		}

		public void ShowInteractPrompt(bool holdToInteract, string? prompt = null)
		{
			if (m_interactPrompt.activeInHierarchy)
				return;

			m_interactPrompt.SetActive(true);
			m_crosshair.SetActive(false);

			if (prompt != null)
				m_interactPromptText.text = prompt;

			m_interactHoldSlider.gameObject.SetActive(holdToInteract);
		}

		public void HideInteractPrompt()
		{
			if (!m_interactPrompt.activeInHierarchy)
				return;

			StopInteractHoldProgress();
			m_interactPromptText.text = "";
			m_interactPrompt.SetActive(false);
			m_crosshair.SetActive(true);

		}

		public void StartInteractHoldProgress(float holdTimeSeconds)
		{
			m_startHoldInteractionTime = DateTime.UtcNow;
			m_endHoldInteractionTime = m_startHoldInteractionTime.Value.AddSeconds(holdTimeSeconds);
			m_interactHoldSlider.value = 0;
		}

		public void StopInteractHoldProgress()
		{
			m_startHoldInteractionTime = null;
			m_endHoldInteractionTime = null;
			m_interactHoldSlider.value = 0;
		}

	}
}
