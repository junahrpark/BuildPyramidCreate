// DiggingMiniGame.cs
using UnityEngine;
using UnityEngine.UI;

public class DiggingMiniGame : MonoBehaviour
{
    // --- (기존 변수 및 함수들은 그대로 둡니다) ---
    public RectTransform needleTransform;
    public float rotationSpeed = 180f;
    public float successStartAngle = 210f;
    public float successEndAngle = 250f;
    public GameObject coverObject;
    private int successCount = 0;
    private float uiActiveTime = 0f;
    private float inputDelay = 0.5f;
    private bool hasExitedSuccessZone = true;

    void OnEnable()
    {
        uiActiveTime = Time.time;
        hasExitedSuccessZone = true;
        // Reset success count when UI becomes active
        successCount = 0;
    }

    void Update()
    {
        // --- (기존 Update 로직은 그대로 둡니다) ---
        needleTransform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);

        if (Time.time - uiActiveTime < inputDelay)
            return;

        float angle = GetNeedleAngle();
        bool isInZone = IsInAngleRange(angle, successStartAngle, successEndAngle);

        if (Input.GetMouseButtonDown(0) && isInZone && hasExitedSuccessZone)
        {
            Debug.Log("🎯 성공!");
            successCount++;
            hasExitedSuccessZone = false;

            if (successCount == 1 || successCount == 2)
            {
                if (coverObject != null) coverObject.transform.localScale *= 0.7f;
            }
            else if (successCount >= 3)
            {
                if (coverObject != null) coverObject.SetActive(false);
                this.gameObject.SetActive(false); // UI 닫기 (이때 OnDisable 호출됨)
            }
        }

        if (!isInZone)
        {
            hasExitedSuccessZone = true;
        }
    }

    float GetNeedleAngle()
    {
        // --- (기존 GetNeedleAngle 로직) ---
        float rawAngle = needleTransform.eulerAngles.z;
        float adjusted = (rawAngle - 270f + 360f) % 360f;
        return adjusted;
    }

    bool IsInAngleRange(float angle, float start, float end)
    {
        // --- (기존 IsInAngleRange 로직) ---
        if (start < end)
            return angle >= start && angle <= end;
        else
            return angle >= start || angle <= end;
    }

    public void SetCoverObject(GameObject cover)
    {
        // --- (기존 SetCoverObject 로직) ---
        coverObject = cover;
        successCount = 0; // Reset count when setting a new object
    }


    // --- 이 함수를 추가합니다 ---
    // 이 UI 오브젝트가 비활성화될 때 자동으로 호출됩니다.
    void OnDisable()
    {
        Debug.Log("DiggingMiniGame UI 비활성화됨. 컨트롤 복구 시도.");

        // 씬에서 플레이어 컨트롤 스크립트들을 찾습니다.
        // (주의: 이 방식은 씬에 해당 타입의 스크립트가 하나만 있다고 가정합니다)
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        MouseLook mouseLook = FindObjectOfType<MouseLook>();
        PlayerInteraction playerInteraction = FindObjectOfType<PlayerInteraction>();

        // 찾은 스크립트들을 다시 활성화합니다. (null 체크 필수)
        if (playerMovement != null) playerMovement.enabled = true;
        if (mouseLook != null) mouseLook.enabled = true;
        if (playerInteraction != null) playerInteraction.enabled = true;

        // 게임이 종료되는 상황 등이 아니라면 커서를 다시 잠급니다.
        // PlayerMovement가 활성화 되어있다는 것은 플레이어 모드로 돌아감을 의미.
        if (playerMovement != null && playerMovement.enabled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    // --------------------------
}