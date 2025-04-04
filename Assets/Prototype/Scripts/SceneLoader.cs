using UnityEngine;
using UnityEngine.SceneManagement; // Scene ������ ���� �� �ʿ��մϴ�!

public class SceneLoader : MonoBehaviour
{
    // ��ư Ŭ�� �� ȣ��� �Լ��Դϴ�.
    // string Ÿ���� �Ű����� sceneName�� �޾� �ش� �̸��� ���� �ε��մϴ�.
    public void LoadSceneByName(string sceneName)
    {
        // sceneName ������ ���޵� �̸��� ���� �ε��մϴ�.
        SceneManager.LoadScene(sceneName);
        Debug.Log(sceneName + " �� �ε� �õ�..."); // �ֿܼ� �α׸� ����Ͽ� Ȯ���մϴ�.
    }

    // (���� ����) ���� �ε����� ���� �ε��ϴ� �Լ�
    // public void LoadSceneByIndex(int sceneBuildIndex)
    // {
    //     SceneManager.LoadScene(sceneBuildIndex);
    //     Debug.Log("���� �ε��� " + sceneBuildIndex + " �� �ε� �õ�...");
    // }

    // (����) ���� ���� �Լ�
    public void QuitGame()
    {
        Debug.Log("���� ���� �õ�...");
        Application.Quit(); // ����� ���ӿ����� �۵��մϴ�. �����Ϳ����� ���õ˴ϴ�.
    }
}