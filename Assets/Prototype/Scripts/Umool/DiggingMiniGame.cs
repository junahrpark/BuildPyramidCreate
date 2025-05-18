using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DiggingMiniGame : MonoBehaviour
{
    public RectTransform needleTransform;
    public float successStartAngle = 285f;
    public float successEndAngle = 350f;

    [Header("사운드 설정")]
    public AudioClip completionSound;

    private GameObject coverObject;
    private string artifactID;
    private AudioSource audioSource;

    private int successCount = 0;
    private float uiActiveTime = 0f;
    private float inputDelay = 0.5f;
    private bool hasExitedSuccessZone = true;
    private bool isComplete = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void OnEnable()
    {
        uiActiveTime = Time.time;
        hasExitedSuccessZone = true;
        successCount = 0;
        isComplete = false;
    }

    void Update()
    {
        if (Time.time - uiActiveTime < inputDelay) return;

        float angle = GetNeedleAngle();
        bool isInZone = IsInAngleRange(angle, successStartAngle, successEndAngle);

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

        if (!isInZone)
            hasExitedSuccessZone = true;

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
            else if (successCount >= 3 && !isComplete)
            {
                isComplete = true;
                Debug.Log("✅ 3회 성공 - 유물 발굴 완료!");

                if (completionSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(completionSound);
                    Debug.Log($"🔊 완료 사운드 재생: {completionSound.name}");
                }
                else
                {
                    if (completionSound == null)
                        Debug.LogWarning("완료 사운드(completionSound)가 할당되지 않았습니다.");
                    if (audioSource == null)
                        Debug.LogWarning("AudioSource 컴포넌트를 찾을 수 없습니다.");
                }

                if (!string.IsNullOrEmpty(artifactID))
                {
                    ArtifactStatusManager.Instance.SetFound(artifactID);
                    Debug.Log($"💾 유물 {artifactID} 저장 완료!");
                }

                coverObject.SetActive(false);
                this.gameObject.SetActive(false);
            }
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

        if (playerMovement != null) playerMovement.canMove = true;
        if (mouseLook != null) mouseLook.enabled = true;
        if (playerInteraction != null) playerInteraction.enabled = true;

        if (playerMovement != null && playerMovement.enabled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
