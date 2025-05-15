// PlayerMovement.cs - CharacterController.isGrounded ��� ����
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;

    // --- CharacterController.isGrounded�� ����ϹǷ� �Ʒ� �������� �ʿ� �����ϴ� ---
    // public Transform groundCheck;
    // public LayerMask groundMask;
    // public float groundDistance = 0.4f;
    // --------------------------------------------------------------------

    CharacterController controller;
    Vector3 velocity;
    // bool isGrounded; // CharacterController�� isGrounded�� ���� ���

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // --- �ٴ� ���� (CharacterController ���� ��� ���) ---
        // ���� Physics.CheckSphere ������ �����ϰ� �Ʒ�ó�� controller.isGrounded ���
        bool isGrounded = controller.isGrounded;

        // ���� ��� �ְ� ���� �ӵ��� 0 �����̸� �ӵ� ���� (�ٴڿ� �پ��ֵ���)
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // --- �̵� ó�� ---
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        // --- ���� ó�� ---
        // Jump ��ư�� ���Ȱ�, CharacterController�� ���� �ִٰ� �Ǵ��ϸ� ����
        if (Input.GetButtonDown("Jump") && isGrounded) // ���⼭ isGrounded�� controller.isGrounded ���Դϴ�.
        {
            // ������ �ʿ��� y�� �ӵ� ���
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // --- �߷� ���� ---
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime); // �߷� �� ���� �ӵ� ����
    }

    // --- OnDrawGizmosSelected �Լ��� �ʿ� �����Ƿ� �����ص� �˴ϴ� ---
    /*
    void OnDrawGizmosSelected()
    {
        // ... (�� �Լ� ���� ����) ...
    }
    */
}