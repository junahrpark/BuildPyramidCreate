using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneRewardActivator : MonoBehaviour
{
    public string targetSceneName = "05 Auto Turning";     // 전환될 씬 이름
    public string targetObjectName = "umoolpng";           // 활성화할 오브젝트 이름

    private static bool shouldActivateReward = false;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject); // 씬 바뀌어도 살아있음
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public static void ActivateInNextScene()
    {
        shouldActivateReward = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (shouldActivateReward && scene.name == targetSceneName)
        {
            // ✅ 비활성 오브젝트까지 포함해서 검색
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            GameObject target = null;

            foreach (GameObject obj in allObjects)
            {
                if (obj.name == targetObjectName && obj.scene.name == targetSceneName)
                {
                    target = obj;
                    break;
                }
            }

            if (target != null)
            {
                target.SetActive(true);
                Debug.Log("✅ 보상 오브젝트 활성화됨: " + target.name);
            }
            else
            {
                Debug.LogWarning($"❌ '{targetObjectName}' 오브젝트를 씬 '{scene.name}'에서 찾지 못했습니다.");
            }

            shouldActivateReward = false;
        }
    }
}
