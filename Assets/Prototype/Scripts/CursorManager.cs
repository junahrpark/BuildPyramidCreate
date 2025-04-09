using UnityEngine;

public class CursorManager : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.None; // 커서 잠금 해제
        Cursor.visible = true;                  // 커서 보이게 설정
    }
}
