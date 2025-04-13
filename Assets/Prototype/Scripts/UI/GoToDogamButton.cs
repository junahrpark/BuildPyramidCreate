using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToDogamButton : MonoBehaviour
{
    public Transform playerTransform;
    public string codexSceneName = "05 Auto Turning";

    void OnMouseDown()
    {
        // 위치 저장 먼저!
        if (SceneTracker.Instance != null && playerTransform != null)
        {
            SceneTracker.Instance.SetReturnPoint(
                SceneManager.GetActiveScene().name,
                playerTransform.position,
                playerTransform.eulerAngles.y
            );

            Debug.Log("위치 저장 완료! → 도감 씬으로 이동");
        }
        else
        {
            Debug.LogWarning("📛 SceneTracker 또는 PlayerTransform이 연결되지 않았습니다!");
        }

        // 씬 이동은 꼭 그 다음에!
        SceneManager.LoadScene(codexSceneName);
    }
}
