using UnityEngine;
using UnityEngine.SceneManagement;

public class Dogam : MonoBehaviour
{
    [Tooltip("전환할 도감 씬 이름")]
    public string codexSceneName = "05 Auto Turning";

    void OnMouseDown()
    {
        // 책 오브젝트 클릭 시 도감 씬으로 전환
        SceneManager.LoadScene(codexSceneName);
        Debug.Log("도감 씬으로 이동 시도: " + codexSceneName);
    }
}
