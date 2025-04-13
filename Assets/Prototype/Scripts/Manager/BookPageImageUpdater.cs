using UnityEngine;
using System.Collections.Generic;
using ScriptBoy.ProceduralBook; // BookContent 접근용

public class BookPageImageUpdater : MonoBehaviour
{
    [Header("발굴된 유물 오브젝트 리스트 (이름 = 유물 ID)")]
    public GameObject[] artifactObjects;

    [Header("각 유물의 정보 이미지 (도감 페이지용)")]
    public Sprite[] pageSprites;

    [Header("대상 BookContent")]
    public BookContent bookContent;

    void Start()
    {
        if (artifactObjects.Length != pageSprites.Length)
        {
            Debug.LogWarning("artifactObjects와 pageSprites 배열 길이가 다릅니다!");
            return;
        }

        var field = typeof(BookContent).GetField("m_Pages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Object[] pages = field.GetValue(bookContent) as Object[];

        for (int i = 0; i < artifactObjects.Length; i++)
        {
            string id = artifactObjects[i].name;

            if (ArtifactStatusManager.Instance.IsFound(id))
            {
                Debug.Log($"✅ 유물 {id} 발굴됨 → 도감 이미지 적용");
                if (i < pages.Length) pages[i] = pageSprites[i];
            }
            else
            {
                Debug.Log($"❓ 유물 {id} 아직 미발굴");
            }
        }

        field.SetValue(bookContent, pages);
    }
}
