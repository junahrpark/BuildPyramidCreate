using UnityEngine;
using UnityEngine.EventSystems; // UI 클릭 감지를 위해 필요

public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;         // 플레이어 카메라 (인스펙터에서 할당)
    public float interactDistance = 100f; // 클릭 가능 거리

    void Awake()
    {
        // 카메라 자동 할당
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogError("PlayerInteraction: Player Camera not found! Please assign it in the Inspector or ensure a camera is tagged 'MainCamera'.");
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 좌클릭
        {
            // UI 클릭 방지
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactDistance))
            {
                // 1. 유물 클릭 즉시 카운트 추가를 위한 스크립트 호출
                ArtifactTouchHandler touchHandler = hit.collider.GetComponent<ArtifactTouchHandler>();
                if (touchHandler != null)
                {
                    touchHandler.TouchArtifact(); // 유물 터치 함수 호출 (카운트 증가 담당)
                }

                // 2. 미니게임 발동을 위한 스크립트 호출
                ArtifactDigController digTarget = hit.collider.GetComponent<ArtifactDigController>();
                if (digTarget != null)
                {
                    digTarget.StartDigUI(); // 클릭된 유물에 미니게임 UI 띄움
                    Debug.Log("PlayerInteraction: Starting dig UI for: " + hit.collider.name);
                }
                else
                {
                    Debug.Log("PlayerInteraction: Clicked on: " + hit.collider.name + " (Not a recognized artifact)");
                }
            }
        }
    }
}