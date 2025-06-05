using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToDogamTrigger : MonoBehaviour // 클래스 이름을 역할에 맞게 변경하는 것이 좋습니다.
{
    public Transform playerTransform;
    public string codexSceneName = "05 Auto Turning";
    public string playerTag = "Player"; // 플레이어 오브젝트에 설정된 태그

    // OnTriggerEnter는 Collider가 다른 Collider에 진입했을 때 호출됩니다.
    // 이 스크립트가 적용된 오브젝트의 Collider에서 'Is Trigger'가 반드시 체크되어 있어야 합니다.
    // 또한, 플레이어 오브젝트에는 Rigidbody와 Collider가 있어야 합니다.
    void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트가 플레이어인지 태그를 통해 확인합니다.
        if (other.CompareTag(playerTag))
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
}