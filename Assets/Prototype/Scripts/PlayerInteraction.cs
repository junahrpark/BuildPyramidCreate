// PlayerInteraction.cs
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;         // 플레이어 카메라
    public float interactDistance = 3f; // 클릭 가능 거리

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 좌클릭
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                ArtifactDigController digTarget = hit.collider.GetComponent<ArtifactDigController>();
                if (digTarget != null)
                {
                    digTarget.StartDigUI(); // 클릭된 유물에 미니게임 UI 띄움
                }
            }
        }
    }
}
