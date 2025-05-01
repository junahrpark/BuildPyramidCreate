using System.Collections.Generic;
using UnityEngine;

public class ArtifactStatusManager : MonoBehaviour
{
    public static ArtifactStatusManager Instance;
    private HashSet<string> foundArtifacts = new HashSet<string>();
    public int totalArtifactCount = 15; // 전체 유물 개수 (실제 개수로 수정 필요)

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 살아있게
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetFound(string id)
    {
        if (!foundArtifacts.Contains(id))
        {
            foundArtifacts.Add(id);
            Debug.Log($"[상태 저장됨] 유물 {id} 발굴됨!");
            // TODO: 필요하다면 여기서 완성률 텍스트 업데이트 함수 호출
        }
    }

    public bool IsFound(string id)
    {
        return foundArtifacts.Contains(id);
    }

    // 완성률 계산 함수
    public float GetCompletionPercentage()
    {
        if (totalArtifactCount <= 0) // 0으로 나누는 경우 방지
        {
            Debug.LogWarning("Total artifact count is not set or is zero.");
            return 0f;
        }

        float percent = (float)foundArtifacts.Count / totalArtifactCount * 100f;
        return percent;
    }
}
