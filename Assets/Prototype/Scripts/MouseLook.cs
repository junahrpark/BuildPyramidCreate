using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f; // 마우스 감도

    public Transform playerBody; // 플레이어 몸체 Transform

    float xRotation = 0f; // 카메라의 상하 회전 각도

    void Start()
    {
        // 게임 시작 시 커서를 화면 중앙에 고정하고 숨깁니다.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 마우스 입력 값을 받습니다.
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 상하 회전(X Rotation) 계산 및 제한
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 카메라가 90도 이상으로 뒤집히지 않도록 제한

        // 카메라의 상하 회전 적용 (카메라 자체에 적용)
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 플레이어 몸체의 좌우 회전 적용 (Player 오브젝트에 적용)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}