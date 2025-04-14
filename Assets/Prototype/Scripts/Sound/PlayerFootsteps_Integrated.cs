using UnityEngine;

// 이 스크립트가 제대로 작동하려면 같은 게임 오브젝트에
// AudioSource와 CharacterController가 반드시 필요합니다.
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CharacterController))]
public class PlayerFootsteps_Integrated : MonoBehaviour // 스크립트 이름 변경 (기존 것과 충돌 방지)
{
    [Header("오디오 설정")]
    // [필수] 인스펙터에서 할당: 발소리를 재생할 AudioSource 컴포넌트
    public AudioSource footstepAudioSource;
    // [필수] 인스펙터에서 할당: 사용할 발소리 오디오 클립들 (최소 1개 이상)
    public AudioClip[] footstepClips;
    // 인스펙터에서 조절: 발소리 사이의 시간 간격 (초 단위)
    [Range(0.1f, 1.0f)]
    public float timeBetweenSteps = 0.4f;

    [Header("플레이어 설정")]
    // [필수] 인스펙터에서 할당하거나 자동으로 찾음: 플레이어의 CharacterController
    // (PlayerMovement.cs가 사용하는 것과 동일한 컴포넌트여야 함)
    public CharacterController characterController;
    // 인스펙터에서 조절: 움직인다고 판단할 최소 입력 크기 (0.1 이면 약간만 움직여도 감지)
    public float movementThreshold = 0.1f;

    private float stepTimer; // 다음 발소리 재생까지 남은 시간
    private bool isGrounded; // 현재 땅에 닿아 있는지? (CharacterController에서 읽어옴)
    private bool wasMovingLastFrame = false; // 이전 프레임에서 움직였는지 체크용

    void Start()
    {
        // --- 컴포넌트 자동 할당 시도 (안전하게 인스펙터에서 직접 할당하는 것을 권장) ---
        if (footstepAudioSource == null)
        {
            Debug.LogWarning("PlayerFootsteps: AudioSource를 자동으로 찾습니다. 인스펙터에서 할당하는 것이 좋습니다.", this);
            footstepAudioSource = GetComponent<AudioSource>();
        }
        if (characterController == null)
        {
            Debug.LogWarning("PlayerFootsteps: CharacterController를 자동으로 찾습니다. 인스펙터에서 할당하는 것이 좋습니다.", this);
            characterController = GetComponent<CharacterController>();
        }

        // --- 필수 항목들이 제대로 설정되었는지 최종 확인 ---
        if (footstepAudioSource == null)
            Debug.LogError("### PlayerFootsteps 오류: 'Footstep Audio Source'가 설정되지 않았습니다! 스크립트가 작동하지 않습니다.", this);
        if (characterController == null)
            Debug.LogError("### PlayerFootsteps 오류: 'Character Controller'가 설정되지 않았습니다! 스크립트가 작동하지 않습니다.", this);
        if (footstepClips == null || footstepClips.Length == 0)
            Debug.LogWarning("### PlayerFootsteps 경고: 'Footstep Clips' 배열이 비어있거나 클립이 할당되지 않았습니다. 발소리가 나지 않을 수 있습니다.", this);

        stepTimer = 0f; // 타이머 초기화
    }

    // Update 함수: 속도 계산 방식을 Input.GetAxis 기반으로 변경
    void Update()
    {
        // 필수 컴포넌트가 없으면 Update 로직 실행 중지 (오류 방지)
        if (characterController == null || footstepAudioSource == null) return;

        // CharacterController로부터 지면 상태 읽기
        isGrounded = characterController.isGrounded;

        // --- 속도 계산 방식 변경: Input Axis 직접 사용 ---
        float inputX = Input.GetAxis("Horizontal"); // 좌/우 입력 (A, D, 화살표 좌/우)
        float inputZ = Input.GetAxis("Vertical");   // 앞/뒤 입력 (W, S, 화살표 위/아래)

        // 입력 벡터의 크기를 속도로 사용 (대각선 이동 시 1보다 클 수 있음)
        // 실제 이동 속도와는 약간 다를 수 있지만, '움직이려는 의도'를 파악하는 데 사용
        float speed = new Vector2(inputX, inputZ).magnitude;
        // --------------------------------------------------

        // 상태 로그 (이제 Speed 값은 입력 기반으로 표시됨. 필요 없으면 주석 처리)
        // Debug.Log($"[상태] Grounded: {isGrounded}, Speed (From Input): {speed.ToString("F2")}, Timer: {stepTimer.ToString("F2")}");

        // 움직임 판단 (입력 기반 속도와 Threshold 비교)
        bool isMoving = isGrounded && speed > movementThreshold;

        if (isMoving)
        {
            // 움직임 감지 로그 (필요시 주석 해제)
            // Debug.Log($"[움직임 감지] 움직이는 중! (Grounded: {isGrounded}, Speed: {speed.ToString("F2")})");

            // 타이머 진행
            stepTimer -= Time.deltaTime;

            // 타이머가 다 되면 발소리 재생
            if (stepTimer <= 0f)
            {
               // Debug.Log("[타이머 만료] 발소리 재생 시도."); // 타이머 만료 시 로그 출력
                PlayFootstepSound();
                // 다음 발소리까지의 시간 설정
                stepTimer = timeBetweenSteps;
                // 약간의 랜덤성을 주면 더 자연스러울 수 있음:
                // stepTimer = timeBetweenSteps * Random.Range(0.8f, 1.2f);
            }
            wasMovingLastFrame = true;
        }
        else
        {
            // 멈췄거나 공중에 있을 때
            if (wasMovingLastFrame) // 방금 멈췄다면
            {
                // Debug.Log("[멈춤 감지] 플레이어가 멈춤."); // 필요시 주석 해제
                // 타이머를 약간만 리셋 (다시 걸을 때 즉시 소리 방지)
                stepTimer = timeBetweenSteps * 0.2f;
            }
            wasMovingLastFrame = false;
        }
    }


    void PlayFootstepSound()
    {
        // 오디오 소스와 클립 배열이 유효한지 최종 확인
        if (footstepAudioSource != null && footstepClips != null && footstepClips.Length > 0)
        {
            // 랜덤 클립 선택
            int index = Random.Range(0, footstepClips.Length);
            AudioClip clipToPlay = footstepClips[index];

            // 선택된 클립이 null이 아닌지 확인
            if (clipToPlay != null)
            {
                // 재생 정보 로그
               // Debug.Log($"[사운드 재생] 재생할 클립: {clipToPlay.name}");
                // 사운드 재생 (볼륨 조절도 여기서 가능: footstepAudioSource.PlayOneShot(clipToPlay, volumeScale);)
                footstepAudioSource.PlayOneShot(clipToPlay);
            }
            else
            {
                // 배열 요소가 null일 경우 경고
                Debug.LogWarning($"### PlayerFootsteps 경고: Footstep Clips 배열의 {index}번 요소가 비어있습니다(null).");
            }
        }
        else
        {
            // 재생 불가 사유 로그
            if (footstepAudioSource == null) Debug.LogWarning("### PlayerFootsteps: PlayFootstepSound() - AudioSource가 null입니다!");
            if (footstepClips == null || footstepClips.Length == 0) Debug.LogWarning("### PlayerFootsteps: PlayFootstepSound() - Footstep Clips 배열이 비어있거나 길이가 0입니다!");
        }
    }
} // End of class