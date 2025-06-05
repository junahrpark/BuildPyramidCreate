using UnityEngine;
using UnityEngine.SceneManagement;

// 이제 이 스크립트 하나가 씬 이동과 이전 씬 기억을 모두 담당합니다.
public class SceneLoader : MonoBehaviour
{
    // static 변수: 씬을 넘나들어도 값이 유지되는 특별한 공용 변수
    private static string previousSceneName;

    /// <summary>
    /// [용도 1] 다음 씬으로 '기억하며' 이동하는 함수
    /// </summary>
    public void LoadSceneAndRemember(string sceneName)
    {
        // 이동하기 직전, 현재 씬의 이름을 static 변수에 저장합니다.
        previousSceneName = SceneManager.GetActiveScene().name;

        // 입력받은 새 씬을 로드합니다.
        SceneManager.LoadScene(sceneName);
        Debug.Log(sceneName + " 씬 로딩! (이전 씬: " + previousSceneName + "을 기억했습니다)");
    }

    /// <summary>
    /// [용도 2] 기억해 둔 '이전 씬으로' 돌아가는 함수
    /// </summary>
    public void LoadPreviousScene()
    {
        // static 변수에 저장된 씬 이름이 있는지 확인합니다.
        if (!string.IsNullOrEmpty(previousSceneName))
        {
            SceneManager.LoadScene(previousSceneName);
        }
        else
        {
            Debug.LogWarning("기억된 이전 씬이 없습니다! (게임을 첫 씬부터 실행했는지 확인하세요)");
        }
    }

    /// <summary>
    /// (기존 기능) 게임 종료 함수
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("게임 종료 시도...");
        Application.Quit();
    }
}