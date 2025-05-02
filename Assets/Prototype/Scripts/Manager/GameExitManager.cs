using UnityEngine;

public class GameExitManager : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("게임 종료 시도됨");

        // 빌드된 실행파일에서는 게임 종료
        Application.Quit();

#if UNITY_EDITOR
        // 에디터에서는 Play 모드 종료
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
