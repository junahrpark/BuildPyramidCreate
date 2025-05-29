using UnityEngine;
using UnityEngine.UI; // UI 요소를 사용하기 위해 필요

public class GameManager : MonoBehaviour
{
    // UI 텍스트 컴포넌트 (인스펙터에서 드래그하여 할당)
    public Text collectCountText;

    // 최대 유물 개수는 10개로 고정 (const는 코드에서 변경 불가)
    public const int TOTAL_ARTIFACTS = 10;

    private int currentCollectedCount = 0; // 현재까지 발견된 유물 개수

    void Awake()
    {
        // 게임 시작 시 총 유물 개수를 자동으로 계산하려면 아래 주석 해제
        // 이 경우 totalCollectables 변수를 사용해야 합니다.
        // totalCollectables = FindObjectsOfType<ArtifactTouchHandler>().Length; 
        // Debug.Log($"[GameManager] Total touchable artifacts found: {totalCollectables}");

        // 10개로 고정할 경우, 위 코드는 필요 없습니다.
        Debug.Log($"[GameManager] Total artifacts fixed to: {TOTAL_ARTIFACTS}");
    }

    void OnEnable()
    {
        // ArtifactTouchHandler의 '유물 터치' 이벤트를 듣습니다.
        ArtifactTouchHandler.OnArtifactTouched += OnArtifactTouchedCallback;
    }

    void OnDisable()
    {
        // 스크립트 비활성화 시 이벤트 듣기를 중지합니다. (메모리 누수 방지)
        ArtifactTouchHandler.OnArtifactTouched -= OnArtifactTouchedCallback;
    }

    void Start()
    {
        // 게임 시작 시 UI를 초기화합니다.
        UpdateCollectCountUI();
    }

    // 유물이 터치될 때마다 ArtifactTouchHandler로부터 호출되는 함수
    private void OnArtifactTouchedCallback()
    {
        // 이미 최대 개수를 넘었다면 더 이상 카운트하지 않습니다.
        if (currentCollectedCount >= TOTAL_ARTIFACTS)
        {
            Debug.LogWarning("[GameManager] Attempted to count artifact, but max already reached.");
            return;
        }

        currentCollectedCount++; // 발견 개수 증가
        UpdateCollectCountUI(); // UI 업데이트

        Debug.Log($"Artifact Touched! Current: {currentCollectedCount} / Total: {TOTAL_ARTIFACTS}");

        // 모든 유물을 다 찾았을 때의 추가 로직 (선택 사항)
        if (currentCollectedCount >= TOTAL_ARTIFACTS)
        {
            Debug.Log("All artifacts have been touched!");
            // 여기에 게임 승리 화면 표시, 다음 레벨 이동 등의 로직을 추가
        }
    }

    // UI 텍스트를 업데이트하는 함수
    private void UpdateCollectCountUI()
    {
        if (collectCountText != null)
        {
            collectCountText.text = $"Found: {currentCollectedCount} / {TOTAL_ARTIFACTS}";
        }
    }
}