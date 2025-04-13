using UnityEngine;

public class SceneTracker : MonoBehaviour
{
    public static SceneTracker Instance;

    private string previousScene = "";
    private Vector3 returnPosition;
    private float returnYRotation;

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

    public void SetReturnPoint(string sceneName, Vector3 position, float yRotation)
    {
        previousScene = sceneName;
        returnPosition = position;
        returnYRotation = yRotation;
    }

    public string GetScene() => previousScene;
    public Vector3 GetPosition() => returnPosition;
    public float GetYRotation() => returnYRotation;
}
