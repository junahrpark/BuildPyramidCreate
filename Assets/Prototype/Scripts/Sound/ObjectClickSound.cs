using UnityEngine;

// 스크립트가 부착될 때 자동으로 AudioSource와 Collider 컴포넌트가 있는지 확인하거나 추가합니다.
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Collider))]
public class ObjectClickSound : MonoBehaviour
{
    private AudioSource audioSource;

    // 선택 사항: 인스펙터에서 특정 클릭 사운드를 지정하고 싶을 경우 사용
    public AudioClip specificClickSound;

    void Start()
    {
        // 이 게임 오브젝트에 부착된 AudioSource 컴포넌트를 가져옵니다.
        audioSource = GetComponent<AudioSource>();

        // 안전 장치: 만약 PlayOnAwake가 켜져 있다면 꺼줍니다.
        if (audioSource.playOnAwake)
        {
            audioSource.playOnAwake = false;
        }
    }

    // 이 오브젝트의 Collider 위에서 마우스 버튼을 눌렀을 때 호출되는 함수입니다.
    void OnMouseDown()
    {
        // 인스펙터에서 specificClickSound를 지정했다면 그것을 재생
        if (specificClickSound != null)
        {
            // PlayOneShot은 사운드가 끝나기 전에 다시 클릭해도 중첩해서 재생됩니다.
            // 효과음에 적합합니다.
            audioSource.PlayOneShot(specificClickSound);
        }
        // 그렇지 않고 AudioSource의 기본 AudioClip이 설정되어 있다면 그것을 재생
        else if (audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
            // 만약 사운드가 겹치지 않고, 이전 재생이 끝나야 다음 재생이 되게 하려면 Play() 사용
            // audioSource.Play();
        }
        else
        {
            // 재생할 오디오 클립이 없는 경우 경고 메시지 출력
            Debug.LogWarning("클릭 사운드를 재생할 AudioClip이 지정되지 않았습니다.", this.gameObject);
        }
    }
}