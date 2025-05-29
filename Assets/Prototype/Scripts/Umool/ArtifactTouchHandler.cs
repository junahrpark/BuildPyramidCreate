using UnityEngine;

public class ArtifactTouchHandler : MonoBehaviour
{
    // GameManager가 이 이벤트를 구독하여 유물이 터치되었음을 알 수 있습니다.
    public static event System.Action OnArtifactTouched;

    private bool hasBeenCounted = false; // 한 번 카운트된 유물은 다시 카운트되지 않게 방지

    // 플레이어의 클릭/터치에 의해 PlayerTouchHandler 스크립트에서 이 함수를 호출합니다.
    public void TouchArtifact()
    {
        if (hasBeenCounted) return; // 이미 카운트했다면 무시

        hasBeenCounted = true; // 카운트됨으로 표시

        Debug.Log("Artifact touched: " + gameObject.name + " (Not yet collected/deactivated)");

        // GameManager에게 유물이 터치되었다고 알립니다.
        if (OnArtifactTouched != null)
        {
            OnArtifactTouched.Invoke();
        }

        // 여기에 터치 시 시각/청각 피드백 추가 가능 (예: 반짝임, 소리)
        // 예시: GetComponent<Renderer>().material.color = Color.yellow; // 터치 시 색 변경
    }
}