using UnityEngine;
using UnityEngine.SceneManagement; // Scene 관리를 위해 꼭 필요합니다!

public class SceneLoader : MonoBehaviour
{
    // 버튼 클릭 시 호출될 함수입니다.
    // string 타입의 매개변수 sceneName을 받아 해당 이름의 씬을 로드합니다.
    public void LoadSceneByName(string sceneName)
    {
        // sceneName 변수에 전달된 이름의 씬을 로드합니다.
        SceneManager.LoadScene(sceneName);
        Debug.Log(sceneName + " 씬 로딩 시도..."); // 콘솔에 로그를 출력하여 확인합니다.
    }

    // (선택 사항) 빌드 인덱스로 씬을 로드하는 함수
    // public void LoadSceneByIndex(int sceneBuildIndex)
    // {
    //     SceneManager.LoadScene(sceneBuildIndex);
    //     Debug.Log("빌드 인덱스 " + sceneBuildIndex + " 씬 로딩 시도...");
    // }

    // (참고) 게임 종료 함수
    public void QuitGame()
    {
        Debug.Log("게임 종료 시도...");
        Application.Quit(); // 빌드된 게임에서만 작동합니다. 에디터에서는 무시됩니다.
    }
}