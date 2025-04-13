using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToLastScene : MonoBehaviour
{
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void BackToLastScene()
    {
        string sceneName = SceneTracker.Instance.GetScene();
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SceneTracker.Instance.GetScene())
        {
            var player = FindObjectOfType<PlayerMovement>();
            if (player != null)
            {
                player.transform.position = SceneTracker.Instance.GetPosition();
                player.transform.eulerAngles = new Vector3(
                    0f,
                    SceneTracker.Instance.GetYRotation(),
                    0f
                );
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
