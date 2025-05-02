using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenuUI; // ESCMenuPanel 할당
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // 게임 일시정지
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
{
    pauseMenuUI.SetActive(false);
    Time.timeScale = 1f;
    isPaused = false;
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;

    Input.ResetInputAxes(); // ✅ 눌렀던 입력 강제 초기화

    EventSystem.current.SetSelectedGameObject(null);
    EventSystem.current.sendNavigationEvents = false;
    EventSystem.current.sendNavigationEvents = true;
}



    public void QuitGame()
    {
        Debug.Log("게임 종료 시도");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("UIGameStart"); // 네 메인메뉴 씬 이름
    }
}
