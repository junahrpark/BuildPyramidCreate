using UnityEngine;
using UnityEngine.UI;

public class DiggingMiniGame : MonoBehaviour
{
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
    }

    void Update()
    {
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
                coverObject.transform.localScale *= 0.7f;
            }
            else if (successCount >= 3)
            {
                coverObject.SetActive(false);
                this.gameObject.SetActive(false);
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

    // ✅ 여기에 추가해야 함!
    public void SetCoverObject(GameObject cover)
    {
        coverObject = cover;
        successCount = 0;
    }
}
