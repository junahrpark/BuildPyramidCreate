using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // UI ���ӽ����̽� �߰�
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Image))] // GUITexture ��� Image ���
public class ForcedReset : MonoBehaviour
{
    private void Update()
    {
        // if we have forced a reset ...
        if (CrossPlatformInputManager.GetButtonDown("ResetObject"))
        {
            //... reload the scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // ��� ��� �� �̸� ���
        }
    }
}
