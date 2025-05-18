using UnityEngine;

public class ArtifactDigController : MonoBehaviour
{
    [Header("유물 고유 ID")]
    [SerializeField] private string artifactID;

    [Header("UI & 대상")]
    public GameObject diggingUI;
    public GameObject coverObject;

    [Header("플레이어 컨트롤 참조")]
    public PlayerMovement playerMovement;
    public MouseLook mouseLook;
    public PlayerInteraction playerInteraction;

    void Start()
    {
        if (!string.IsNullOrEmpty(artifactID))
        {
            if (ArtifactStatusManager.Instance.IsFound(artifactID))
            {
                Debug.Log($"💎 유물 {artifactID}는 이미 발굴됨 → 커버 비활성화");
                if (coverObject != null)
                {
                    coverObject.SetActive(false);
                }
            }
        }
    }

    public void StartDigUI()
    {
        Debug.Log("StartDigUI 함수 호출됨! 대상: " + gameObject.name);

        if (coverObject == null)
            coverObject = this.gameObject;

        if (diggingUI != null && !diggingUI.activeSelf)
        {
            Debug.Log("컨트롤 비활성화 및 커서 잠금 해제 시도...");

            if (playerMovement != null) playerMovement.canMove = false;
            if (mouseLook != null) mouseLook.enabled = false;
            if (playerInteraction != null) playerInteraction.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            diggingUI.SetActive(true);

            DiggingMiniGame miniGame = diggingUI.GetComponent<DiggingMiniGame>();
            if (miniGame != null)
            {
                // ✅ 새 함수 사용
                miniGame.SetArtifactInfo(coverObject, artifactID);
            }
            else
            {
                Debug.LogError("DiggingUI에 DiggingMiniGame이 없습니다!", diggingUI);
            }
        }
        else if (diggingUI == null)
        {
            Debug.LogError("DiggingUI가 " + gameObject.name + "의 ArtifactDigController에 할당되지 않았습니다!");
        }
    }
}
