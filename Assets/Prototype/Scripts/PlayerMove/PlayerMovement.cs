// PlayerMovement.cs - CharacterController.isGrounded 사용 버전
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;

    // --- CharacterController.isGrounded를 사용하므로 아래 변수들은 필요 없습니다 ---
    // public Transform groundCheck;
    // public LayerMask groundMask;
    // public float groundDistance = 0.4f;
    // --------------------------------------------------------------------

    CharacterController controller;
    Vector3 velocity;
    // bool isGrounded; // CharacterController의 isGrounded를 직접 사용

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // --- 바닥 감지 (CharacterController 내장 기능 사용) ---
        // 이전 Physics.CheckSphere 라인을 삭제하고 아래처럼 controller.isGrounded 사용
        bool isGrounded = controller.isGrounded;

        // 땅에 닿아 있고 수직 속도가 0 이하이면 속도 리셋 (바닥에 붙어있도록)
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // --- 이동 처리 ---
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        // --- 점프 처리 ---
        // Jump 버튼이 눌렸고, CharacterController가 땅에 있다고 판단하면 점프
        if (Input.GetButtonDown("Jump") && isGrounded) // 여기서 isGrounded는 controller.isGrounded 값입니다.
        {
            // 점프에 필요한 y축 속도 계산
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // --- 중력 적용 ---
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime); // 중력 및 점프 속도 적용
    }

    // --- OnDrawGizmosSelected 함수도 필요 없으므로 삭제해도 됩니다 ---
    /*
    void OnDrawGizmosSelected()
    {
        // ... (이 함수 내용 삭제) ...
    }
    */
}