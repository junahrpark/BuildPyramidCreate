using UnityEngine;
using UnityEngine.EventSystems; // UI 클릭 감지를 위해 필요

public class PlayerTouchHandler : MonoBehaviour
{
    public Camera mainCamera; // 씬의 메인 카메라 (인스펙터에서 드래그하여 할당)
    public float interactionDistance = 100f; // 상호작용 가능한 최대 거리

    void Awake()
    {
        // 자동 할당: 메인 카메라가 할당되지 않았다면 'MainCamera' 태그를 가진 카메라를 찾습니다.
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main Camera not found! Please assign it or ensure a camera is tagged 'MainCamera'.");
            }
        }
    }

    void Update()
    {
        // 마우스 왼쪽 버튼 클릭 또는 모바일 터치 감지
        if (Input.GetMouseButtonDown(0))
        {
            // UI 클릭 여부 확인 (UI를 클릭했다면 게임 오브젝트 상호작용 무시)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                //Debug.Log("UI was clicked, ignoring game object interaction.");
                return;
            }

            // 마우스/터치 위치에서 레이저 발사
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 레이저에 오브젝트가 맞았는지 확인 (Collider 필요)
            if (Physics.Raycast(ray, out hit, interactionDistance))
            {
                // 충돌한 오브젝트에 ArtifactTouchHandler 스크립트가 있는지 확인
                ArtifactTouchHandler artifactTouch = hit.collider.GetComponent<ArtifactTouchHandler>();
                if (artifactTouch != null)
                {
                    artifactTouch.TouchArtifact(); // 유물 터치 함수 호출
                }
                else
                {
                    Debug.Log("Clicked on: " + hit.collider.name + " (Not a touchable artifact)");
                }
            }
        }
    }
}