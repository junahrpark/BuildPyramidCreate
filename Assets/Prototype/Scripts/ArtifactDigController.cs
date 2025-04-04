// ArtifactDigController.cs
using UnityEngine;

public class ArtifactDigController : MonoBehaviour
{
    [Header("UI & 대상")] // Inspector에서 보기 좋게 그룹화
    public GameObject diggingUI;       // 공통 DiggingUI (Inspector에서 할당)
    public GameObject coverObject;     // 나 자신 또는 파괴될 오브젝트 (Inspector에서 할당)

    [Header("플레이어 컨트롤 참조")] // Inspector에서 할당 필요!
    public PlayerMovement playerMovement;    // Player 오브젝트에 있는 PlayerMovement 할당
    public MouseLook mouseLook;          // Main Camera 오브젝트에 있는 MouseLook 할당
    public PlayerInteraction playerInteraction; // Player 오브젝트에 있는 PlayerInteraction 할당

    /* // OnMouseDown은 FPS 방식에서 사용 안 함
    void OnMouseDown()
    {
        // ... (삭제 또는 주석 처리) ...
    }
    */

    // FPS 방식 Raycast 또는 다른 외부 스크립트에서 호출될 함수
    public void StartDigUI()
    {
        // 이 Debug.Log 라인을 추가하세요!
        Debug.Log("StartDigUI 함수 호출됨! 대상: " + gameObject.name);
        // coverObject가 할당 안됐으면 자기 자신으로 기본 설정 (선택적)
        if (coverObject == null) {
            coverObject = this.gameObject;
        }

        // Digging UI가 존재하고, 비활성화 상태일 때만 실행
        if (diggingUI != null && !diggingUI.activeSelf)
        {
            // --- 이 Debug.Log 라인을 추가하세요! ---
            Debug.Log("컨트롤 비활성화 및 커서 잠금 해제 시도...");

            // --- 플레이어 컨트롤 비활성화 및 커서 잠금 해제 ---
            if (playerMovement != null) playerMovement.enabled = false; else Debug.LogWarning("PlayerMovement 참조가 없습니다.", this);
            if (mouseLook != null) mouseLook.enabled = false; else Debug.LogWarning("MouseLook 참조가 없습니다.", this);
            if (playerInteraction != null) playerInteraction.enabled = false; else Debug.LogWarning("PlayerInteraction 참조가 없습니다.", this);

            Cursor.lockState = CursorLockMode.None; // 커서 잠금 해제
            Cursor.visible = true; // 커서 보이기
            // -------------------------------------------------

            diggingUI.SetActive(true); // 미니게임 UI 활성화

            // 미니게임 스크립트에 정보 전달
            DiggingMiniGame miniGame = diggingUI.GetComponent<DiggingMiniGame>();
            if (miniGame != null)
            {
                miniGame.SetCoverObject(coverObject);
            }
            else
            {
                Debug.LogError("DiggingUI 오브젝트에 DiggingMiniGame 스크립트가 없습니다!", diggingUI);
            }
        }
        else if (diggingUI == null)
        {
            Debug.LogError("DiggingUI가 " + gameObject.name + "의 ArtifactDigController에 할당되지 않았습니다!");
        }
    }
}