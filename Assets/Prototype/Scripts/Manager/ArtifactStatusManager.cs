using System.Collections.Generic;
using UnityEngine;

public class ArtifactStatusManager : MonoBehaviour
{
    public static ArtifactStatusManager Instance;
    private HashSet<string> foundArtifacts = new HashSet<string>();

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
        }
    }

    public bool IsFound(string id)
    {
        return foundArtifacts.Contains(id);
    }
}
