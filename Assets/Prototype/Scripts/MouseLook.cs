using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f; // ���콺 ����

    public Transform playerBody; // �÷��̾� ��ü Transform

    float xRotation = 0f; // ī�޶��� ���� ȸ�� ����

    void Start()
    {
        // ���� ���� �� Ŀ���� ȭ�� �߾ӿ� �����ϰ� ����ϴ�.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // ���콺 �Է� ���� �޽��ϴ�.
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // ���� ȸ��(X Rotation) ��� �� ����
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // ī�޶� 90�� �̻����� �������� �ʵ��� ����

        // ī�޶��� ���� ȸ�� ���� (ī�޶� ��ü�� ����)
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // �÷��̾� ��ü�� �¿� ȸ�� ���� (Player ������Ʈ�� ����)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}