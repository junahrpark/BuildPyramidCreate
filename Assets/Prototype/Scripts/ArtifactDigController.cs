using UnityEngine;

public class ArtifactDigController : MonoBehaviour
{
    public GameObject diggingUI;       // 공통 DiggingUI
    public GameObject coverObject;     // 나 자신

    // 마우스 클릭용
    void OnMouseDown()
    {
        // ✅ UI가 이미 떠 있으면 무시 (중복 클릭 방지)
        if (diggingUI != null && !diggingUI.activeSelf)
        {
            StartDigUI();
        }
    }

    // FPS 방식 Raycast에서 호출될 함수
    public void StartDigUI()
    {
        if (diggingUI != null)
        {
            diggingUI.SetActive(true);

            // DiggingUI에 있는 DiggingMiniGame에 발굴 대상 설정
            DiggingMiniGame miniGame = diggingUI.GetComponent<DiggingMiniGame>();
            if (miniGame != null)
            {
                miniGame.SetCoverObject(coverObject);
            }
        }
    }
}
