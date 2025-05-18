using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("이동/점프 설정")]
    public float speed = 6f;
    public float jumpHeight = 3f;

    [Header("중력 설정")]
    public float gravity = -9.81f * 2f;

    [Header("입력 제어")]
    [Tooltip("false면 이동·점프·중력 모두 멈춤, 땅에 붙어만 있음")]
    public bool canMove = true;

    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 1) 땅에 닿았을 때 y속도 리셋 (항상)
        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // 2) 이동 & 점프 (canMove == true일 때만)
        if (canMove)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            Vector3 move = transform.right * x + transform.forward * z;
            controller.Move(move * speed * Time.deltaTime);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        // 3) 중력 적용
        if (canMove)
        {
            // 평소 중력
            velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            // UI 띄워놓은 동안엔 중력 없이 그냥 땅에 붙어 있게
            velocity.y = isGrounded ? -2f : 0f;
        }

        // 4) 항상 Move 호출해서 충돌 유지
        controller.Move(velocity * Time.deltaTime);
    }
}
