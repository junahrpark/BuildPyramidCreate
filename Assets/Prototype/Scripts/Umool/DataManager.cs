using UnityEngine;
using System.Collections.Generic;

// 예시: 간단히 bool 상태 (클리어 여부)만 저장
// 실제로는 YourObjectStateType 같은 커스텀 클래스/구조체를 사용할 수 있습니다.
using YourObjectStateType = System.Boolean;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    // Key: 오브젝트의 고유 ID, Value: 오브젝트의 상태 (예: 클리어 여부)
    public Dictionary<string, YourObjectStateType> taggedObjectStates = new Dictionary<string, YourObjectStateType>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetObjectState(string objectId, YourObjectStateType state)
    {
        taggedObjectStates[objectId] = state;
        Debug.Log($"State saved for {objectId}: {state}");
    }

    public bool TryGetObjectState(string objectId, out YourObjectStateType state)
    {
        return taggedObjectStates.TryGetValue(objectId, out state);
    }
}