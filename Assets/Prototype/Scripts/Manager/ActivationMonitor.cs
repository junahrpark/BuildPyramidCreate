using UnityEngine;
using System.Collections.Generic; // List를 사용하기 위함
using System.Linq; // Linq의 All() 메서드를 사용하면 더 간결하게 표현 가능 (선택 사항)

public class ActivationMonitor : MonoBehaviour
{
    // 인스펙터에서 상태를 감시할 10개의 게임 오브젝트를 할당합니다.
    public List<GameObject> objectsToMonitor;

    // 인스펙터에서 사라지게 할 벽(wall) 오브젝트를 할당합니다.
    public GameObject wallObject;

    // 벽이 이미 사라졌는지 확인하기 위한 플래그 (Update에서 불필요한 반복 체크 방지)
    private bool wallHasBeenDeactivated = false;

    void Start()
    {
        // 필수 항목들이 할당되지 않았으면 경고를 표시하고 스크립트를 비활성화합니다.
        if (objectsToMonitor == null || objectsToMonitor.Count == 0)
        {
            Debug.LogError("ActivationMonitor: 'Objects To Monitor' 리스트가 비어있거나 할당되지 않았습니다!", this);
            enabled = false; // 스크립트 비활성화
            return;
        }
        if (wallObject == null)
        {
            Debug.LogError("ActivationMonitor: 'Wall Object'가 할당되지 않았습니다!", this);
            enabled = false; // 스크립트 비활성화
            return;
        }

        // 게임 시작 시점에서도 한번 상태를 체크합니다.
        // (만약 모든 오브젝트가 이미 비활성화된 상태로 씬이 시작될 경우를 대비)
        CheckObjectStates();
    }

    void Update()
    {
        // 벽이 아직 사라지지 않았다면 매 프레임 상태를 체크합니다.
        if (!wallHasBeenDeactivated)
        {
            CheckObjectStates();
        }
    }

    void CheckObjectStates()
    {
        // 방법 1: 반복문 사용
        bool allObjectsInactive = true;
        foreach (GameObject obj in objectsToMonitor)
        {
            if (obj == null) // 리스트에 할당된 오브젝트가 파괴된 경우 등을 대비
            {
                Debug.LogWarning("ActivationMonitor: 감시 대상 오브젝트 중 하나가 null입니다. 리스트를 확인해주세요.", this);
                allObjectsInactive = false; // 문제가 있으므로 일단 false 처리
                break;
            }

            if (obj.activeSelf) // activeSelf는 인스펙터 체크박스가 켜져있으면 true, 꺼져있으면 false 입니다.
            {
                allObjectsInactive = false; // 하나라도 활성화되어 있다면 조건을 만족하지 않음
                break;
            }
        }

        // 방법 2: LINQ 사용 (더 간결하지만, 많은 수의 오브젝트에는 미세한 성능 차이가 있을 수 있음)
        // if (objectsToMonitor.All(obj => obj != null && !obj.activeSelf))
        // {
        //     // 모든 오브젝트가 null이 아니고 비활성화 상태라면
        //     DeactivateWall();
        // }

        if (allObjectsInactive)
        {
            DeactivateWall();
        }
    }

    void DeactivateWall()
    {
        if (wallObject != null && wallObject.activeSelf) // 벽이 존재하고 아직 활성화 상태라면
        {
            wallObject.SetActive(false); // 벽을 비활성화하여 사라지게 함
            wallHasBeenDeactivated = true;   // 벽이 사라졌음을 표시
            Debug.Log("모든 감시 대상 오브젝트가 비활성화되어 벽을 제거했습니다.");

            // 선택 사항: 임무를 완료했으므로 이 스크립트 자체를 비활성화할 수 있습니다.
            // enabled = false;
        }
    }
}