using UnityEngine;

// 스크립트가 붙으면 자동으로 AudioSource와 Collider 컴포넌트가 있는지 확인하거나 추가합니다.
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Collider))]
public class ObjectClickSound : MonoBehaviour
{
    private AudioSource audioSource;

    // 옵션: 인스펙터에서 특정 클릭 사운드를 지정하여 사용 가능
    public AudioClip specificClickSound;

    void Start()
    {
        // 이 게임 오브젝트에 붙어있는 AudioSource 컴포넌트를 가져옵니다.
        audioSource = GetComponent<AudioSource>();

        // 안전 장치: 만약 PlayOnAwake가 켜져 있다면 끕니다.
        if (audioSource.playOnAwake)
        {
            audioSource.playOnAwake = false;
        }
    }

    // 이 게임오브젝트의 Collider 영역을 마우스 버튼으로 누르면 호출되는 함수입니다.
    void OnMouseDown()
    {
        AudioClip clipToPlay = null;

        // 인스펙터에서 specificClickSound가 지정되었다면 그것을 사용
        if (specificClickSound != null)
        {
            clipToPlay = specificClickSound;
        }
        // 그렇지 않고 AudioSource에 기본 AudioClip이 설정되어 있다면 그것을 사용
        else if (audioSource.clip != null)
        {
            clipToPlay = audioSource.clip;
        }

        // 재생할 클립이 있고 AudioSource가 유효한 경우
        if (clipToPlay != null && audioSource != null)
        {
            // AudioSource의 현재 활성화 상태를 저장합니다.
            bool wasEnabled = audioSource.enabled;

            // 만약 AudioSource가 비활성화 상태였다면,
            if (!wasEnabled)
            {
                // 잠시 활성화합니다.
                audioSource.enabled = true;
            }

            // 소리를 재생합니다.
            // PlayOneShot은 AudioSource가 활성화된 상태에서 호출되어야 하지만,
            // 일단 재생이 시작되면 AudioSource가 다시 비활성화되어도 소리는 끝까지 납니다.
            audioSource.PlayOneShot(clipToPlay);

            // 만약 원래 비활성화 상태였다면,
            if (!wasEnabled)
            {
                // 다시 비활성화 상태로 되돌립니다.
                audioSource.enabled = false;
            }
        }
        else
        {
            // 재생할 오디오 클립이 없는 경우 경고 메시지 출력
            if(clipToPlay == null)
                Debug.LogWarning("클릭 시 재생할 AudioClip이 지정되지 않았습니다.", this.gameObject);
            if(audioSource == null)
                 Debug.LogWarning("AudioSource 컴포넌트를 찾을 수 없습니다.", this.gameObject);
        }
    }
}