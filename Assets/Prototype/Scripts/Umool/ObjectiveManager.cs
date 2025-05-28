using UnityEngine;
using System.Collections.Generic; // List를 사용하기 위해 추가
using System.Linq; // Linq의 All() 메서드를 사용하기 위해 추가

public class ObjectiveManager : MonoBehaviour
{
    public List<TargetObject> targetObjects; // 인스펙터에서 10개의 TargetObject를 할당
    public GameObject doorToUnlock;          // 인스펙터에서 사라지게 할 문 오브젝트를 할당
    public int objectivesRequired;           // 필요한 목표 개수 (자동으로 targetObjects.Count로 설정 가능)

    private int completedObjectivesCount = 0; // 완료된 목표 개수 (선택적 최적화)

    void Start()
    {
        if (targetObjects == null || targetObjects.Count == 0)
        {
            Debug.LogError("목표 오브젝트들이 ObjectiveManager에 할당되지 않았습니다!");
            return;
        }

        objectivesRequired = targetObjects.Count; // 필요한 목표 개수를 리스트 크기로 설정

        // 각 TargetObject의 onCompleted 이벤트에 CheckCompletionStatus 메서드를 리스너로 등록
        foreach (TargetObject obj in targetObjects)
        {
            if (obj != null)
            {
                obj.onCompleted.AddListener(CheckCompletionStatus);
                // 초기 상태 반영 (만약 이미 완료된 상태로 시작하는 오브젝트가 있다면)
                if (obj.isCompleted)
                {
                    completedObjectivesCount++;
                }
            }
        }

        // 시작 시점에서도 한번 상태 체크 (이미 모든 조건이 만족된 상태일 수도 있음)
        CheckCompletionStatus();
    }

    // TargetObject에서 onCompleted 이벤트가 발생하면 호출될 메서드
    public void CheckCompletionStatus()
    {
        // 방법 1: 매번 전체 리스트를 순회하며 완료된 개수 확인
        // completedObjectivesCount = 0;
        // foreach (TargetObject obj in targetObjects)
        // {
        //     if (obj != null && obj.isCompleted)
        //     {
        //         completedObjectivesCount++;
        //     }
        // }

        // 방법 2: Linq 사용 (더 간결함)
        int currentCompletedCount = targetObjects.Count(obj => obj != null && obj.isCompleted);
        // Debug.Log("현재 완료된 목표 수: " + currentCompletedCount + "/" + objectivesRequired);

        if (currentCompletedCount >= objectivesRequired)
        {
            UnlockDoor();
        }
    }

    // (대안) onCompleted에서 호출될 때 단순히 카운트만 증가시키는 방법
    // public void IncrementCompletedCount()
    // {
    //    completedObjectivesCount++;
    //    Debug.Log("완료된 목표 수: " + completedObjectivesCount + "/" + objectivesRequired);
    //    if (completedObjectivesCount >= objectivesRequired)
    //    {
    //        UnlockDoor();
    //    }
    // }


    void UnlockDoor()
    {
        if (doorToUnlock != null && doorToUnlock.activeSelf) // 문이 존재하고 아직 활성화 상태라면
        {
            Debug.Log("모든 목표 완료! 문을 엽니다.");
            doorToUnlock.SetActive(false); // 문 오브젝트를 비활성화하여 사라지게 함
            // 또는 문 여는 애니메이션 재생, 특정 컴포넌트 활성화 등
        }
    }
}