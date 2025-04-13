using UnityEngine;
using System;

public class DiggingMiniGame : MonoBehaviour
{
    public RectTransform needleTransform;
    public float rotationSpeed = 180f;
    public float successStartAngle = 210f;
    public float successEndAngle = 250f;

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
        needleTransform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);

        if (Time.time - uiActiveTime < inputDelay)
            return;

        float angle = GetNeedleAngle();
        bool isInZone = IsInAngleRange(angle, successStartAngle, successEndAngle);

        if (Input.GetMouseButtonDown(0))
        {
            if (isInZone && hasExitedSuccessZone)
            {
                successCount++;
                Debug.Log($"🎯 성공 카운트: {successCount}");
                hasExitedSuccessZone = false;

                if (successCount == 1 || successCount == 2)
                {
                    if (coverObject != null) coverObject.transform.localScale *= 0.7f;
                }
                else if (successCount == 3)
                {
                    Debug.Log("✅ 3회 성공! 유물 발굴 완료!");

                    // ✅ 여기서 직접 상태 저장!
                    if (!string.IsNullOrEmpty(artifactID))
                    {
                        ArtifactStatusManager.Instance.SetFound(artifactID);
                        Debug.Log($"💾 유물 {artifactID} 저장 완료!");
                    }

                    if (coverObject != null) coverObject.SetActive(false);
                    this.gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.Log("❌ 실패 구간 클릭");
            }
        }

        if (!isInZone)
        {
            hasExitedSuccessZone = true;
        }
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

    // ✅ 새로 추가된 함수
    public void SetArtifactInfo(GameObject cover, string id)
    {
        coverObject = cover;
        artifactID = id;
        successCount = 0;
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
