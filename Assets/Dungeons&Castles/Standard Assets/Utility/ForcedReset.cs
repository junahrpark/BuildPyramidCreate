using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // UI 네임스페이스 추가
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Image))] // GUITexture 대신 Image 사용
public class ForcedReset : MonoBehaviour
{
    private void Update()
    {
        // if we have forced a reset ...
        if (CrossPlatformInputManager.GetButtonDown("ResetObject"))
        {
            //... reload the scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 경로 대신 씬 이름 사용
        }
    }
}
