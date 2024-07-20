using System.Collections;
using UnityEngine;
using Fusion;
using Zenject;

namespace FourFathers
{
	public sealed class SceneLoadInfo
	{
        public SceneRef TargetScene;
        public Coroutine LoadRoutine;
        public bool HasSceneLoadFinished;
        public bool HasTransitionSceneLoadFinished;
        public int SceneBuildIndex;
	}

	public class CustomSceneManager : Fusion.NetworkSceneManagerDefault
    {


        [Inject]
        private NetworkManager m_networkManager;

        private SceneLoadInfo m_sceneLoadInfo;
        private SceneRef? m_transitionSceneRef;


		public bool IsLoadingScene => m_sceneLoadInfo != null;

        private void OnIjnect([Inject(Optional = true)] SceneRef transitionScene)
        {
            if(transitionScene.IsValid)
            {
                m_transitionSceneRef = transitionScene;
            }
        }

        public void LoadScene(int sceneBuildIndex)
        {
            if(IsLoadingScene)
            {
                Debug.LogError("Failed to load scene: a scene is already being loaded!");
                return;
            }

			if (CanReloadCurrentScene() == false)
			{
				Debug.LogError("Failed to reload current scene!");
				return;
			}

            m_sceneLoadInfo = new SceneLoadInfo
            {
                TargetScene = SceneRef.FromIndex(sceneBuildIndex),
				SceneBuildIndex = sceneBuildIndex,
				HasSceneLoadFinished = false,
            };

            m_sceneLoadInfo.LoadRoutine = StartCoroutine( LoadSceneRoutine(m_sceneLoadInfo));   
		}



        private bool CanReloadCurrentScene()
        {
            if(m_networkManager.NetworkStatus == NetworkStatus.Disconnected)
            {
                return true;
            }

            return m_transitionSceneRef.HasValue;
        }

        private IEnumerator LoadSceneRoutine(SceneLoadInfo sceneLoadInfo)
        {
            if(m_networkManager.NetworkStatus == NetworkStatus.Connected)
            {
                m_networkManager.NetworkRunner.LoadScene(sceneLoadInfo.TargetScene);

				while (!sceneLoadInfo.HasSceneLoadFinished)
				{
					yield return null;
				}
            }
            else
            {
                yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneLoadInfo.SceneBuildIndex);
            }

            m_sceneLoadInfo = null;
        }
    }
}
