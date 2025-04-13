using UnityEngine;

public class DiggingMiniGame : MonoBehaviour
{
    public RectTransform needleTransform;
    public float successStartAngle = 285f;
    public float successEndAngle = 350f;

    private GameObject coverObject;
    private string artifactID;

    private int successCount = 0;
    private float uiActiveTime = 0f;
    private float inputDelay = 0.5f;
    private bool hasExitedSuccessZone = true;

    void OnEnable()
    {
        uiActiveTime = Time.time;
        hasExitedSuccessZone = true;
        successCount = 0;
    }

    void Update()
    {

        // 입력 딜레이
        if (Time.time - uiActiveTime < inputDelay)
    return;

float angle = GetNeedleAngle();
bool isInZone = IsInAngleRange(angle, successStartAngle, successEndAngle);

// 🔽 여기 로그만 한 줄 추가!
//Debug.Log($"🧭 현재 바늘 각도: {angle}");

// 마우스 클릭 성공 판정
if (Input.GetMouseButtonDown(0))
{
    if (isInZone && hasExitedSuccessZone)
    {
        hasExitedSuccessZone = false;
        successCount++;
        Debug.Log($"🎯 성공 카운트: {successCount}");
    }
    else
    {
        Debug.Log("❌ 실패 구간 클릭");
    }
}


        // 성공 구간 벗어남 체크
        if (!isInZone)
            hasExitedSuccessZone = true;

        // ✅ 성공 횟수에 따라 커버 줄이기 & 처리
        if (coverObject != null)
        {
            if (successCount == 1)
            {
                coverObject.transform.localScale = Vector3.one * 0.7f;
                Debug.Log("📏 커버 크기 1단계 축소");
            }
            else if (successCount == 2)
            {
                coverObject.transform.localScale = Vector3.one * 0.49f;
                Debug.Log("📏 커버 크기 2단계 축소");
            }
            else if (successCount >= 3)
            {
                Debug.Log("✅ 3회 성공 - 유물 발굴 완료!");

                if (!string.IsNullOrEmpty(artifactID))
                {
                    ArtifactStatusManager.Instance.SetFound(artifactID);
                    Debug.Log($"💾 유물 {artifactID} 저장 완료!");
                }

                coverObject.SetActive(false);
                this.gameObject.SetActive(false);
            }
        }
        /*else
        {
            Debug.LogError("❌ coverObject가 설정되지 않았습니다!");
        }*/
    }

    float GetNeedleAngle()
    {
        float rawAngle = needleTransform.eulerAngles.z;
        float adjusted = (rawAngle - 270f + 360f) % 360f;
        return adjusted;
    }

    bool IsInAngleRange(float angle, float start, float end)
    {
        if (start < end)
            return angle >= start && angle <= end;
        else
            return angle >= start || angle <= end;
    }

    public void SetArtifactInfo(GameObject cover, string id)
    {
        coverObject = cover;
        artifactID = id;
        successCount = 0;

        Debug.Log($"🎯 유물 정보 설정됨: ID={artifactID}, 오브젝트={coverObject?.name}");
    }

    void OnDisable()
    {
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        MouseLook mouseLook = FindObjectOfType<MouseLook>();
        PlayerInteraction playerInteraction = FindObjectOfType<PlayerInteraction>();

        if (playerMovement != null) playerMovement.enabled = true;
        if (mouseLook != null) mouseLook.enabled = true;
        if (playerInteraction != null) playerInteraction.enabled = true;

        if (playerMovement != null && playerMovement.enabled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
