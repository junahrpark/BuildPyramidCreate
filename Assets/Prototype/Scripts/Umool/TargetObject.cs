using UnityEngine;
using UnityEngine.Events; // UnityEvent를 사용하기 위해 추가

public class TargetObject : MonoBehaviour
{
    public bool isCompleted = false; // 이 오브젝트가 완료되었는지 여부
    public UnityEvent onCompleted;   // 완료되었을 때 호출될 이벤트 (매니저에게 알리기 위함)

    // 플레이어와의 상호작용 등으로 이 메서드가 호출된다고 가정
    public void CompleteObjective()
    {
        if (!isCompleted) // 아직 완료되지 않았다면
        {
            isCompleted = true;
            Debug.Log(gameObject.name + " 목표 완료!");

            // 외형 변경 등 시각적 피드백 (선택 사항)
            // GetComponent<Renderer>().material.color = Color.green; // 예: 색상 변경

            onCompleted.Invoke(); // 등록된 리스너(매니저의 메서드) 호출
            // gameObject.SetActive(false); // 완료 후 오브젝트를 사라지게 하려면
        }
    }

    // (선택 사항) 플레이어가 이 오브젝트와 상호작용하는 방법
    // 예: 플레이어가 근처에서 'E' 키를 누르면 완료
    // void OnTriggerStay(Collider other)
    // {
    //     if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
    //     {
    //         CompleteObjective();
    //     }
    // }

    // (선택 사항) UI 체크박스와 연동하는 경우
    // public void OnCheckboxValueChanged(bool value)
    // {
    //     if (value) // 체크박스가 true (체크됨)가 되면 완료로 처리
    //     {
    //         CompleteObjective();
    //     }
    // }
}