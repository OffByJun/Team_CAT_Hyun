using _001_Scripts.Manager.Base;
using UnityEditor;
using UnityEngine;

namespace _001_Scripts.Manager
{
    public class SceneManager : SinManagerBase<SceneManager>
    {
        // Scene을 메인 스레드에서 로딩합니다.
        public void LoadScene(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }

        // Scene을 비동기로 언로드합니다.
        public void UnloadSceneAsync(string sceneName)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == sceneName)
                return;
            
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
        }
        
        /// <summary>
        /// Scene을 비동기로 로드합니다.
        /// </summary>
        /// <param name="sceneName">Scene 이름</param>
        /// <returns>로드 진행도</returns>
        public AsyncOperation LoadSceneAsync(string sceneName)
        {
            AsyncOperation p = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            return p;
        }
    }
}