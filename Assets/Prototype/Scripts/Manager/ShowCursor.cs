using UnityEngine;

public class ShowCursor : MonoBehaviour
{
    // 이 스크립트가 포함된 씬이 시작될 때 딱 한 번 실행됩니다.
    void Start()
    {
        // 1. 커서를 다시 보이게 만듭니다.
        Cursor.visible = true;

        // 2. 커서의 잠금 상태를 풀어주어 자유롭게 움직일 수 있게 합니다. (가장 중요)
        Cursor.lockState = CursorLockMode.None;
    }
}