using UnityEngine;
using System.Collections.Generic;
using ScriptBoy.ProceduralBook;
using TMPro;
using System.Reflection;

public class BookPageImageUpdater : MonoBehaviour
{
    [Header("발굴된 유물 오브젝트 리스트 (이름 = 유물 ID)")]
    public GameObject[] artifactObjects;

    [Header("각 유물의 정보 이미지 (도감 페이지용)")]
    public Sprite[] pageSprites;

    [Header("대상 BookContent")]
    public BookContent bookContent;

    [Header("완성률 표시 텍스트")]
    public TMP_Text completionText;

    void Start()
    {
        if (artifactObjects.Length != pageSprites.Length)
        {
            Debug.LogWarning("❗ artifactObjects와 pageSprites 배열 길이가 다릅니다!");
            return;
        }

        if (bookContent == null)
        {
            Debug.LogError("❌ bookContent가 Inspector에 연결되지 않았습니다!");
            return;
        }

        // 필드 존재 확인
        FieldInfo field = typeof(BookContent).GetField("m_Pages", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null)
        {
            Debug.LogError("❌ BookContent에 m_Pages 필드가 없습니다. ScriptBoy 에셋 버전이 다를 수 있습니다.");
            return;
        }

        Object[] pages = field.GetValue(bookContent) as Object[];
        if (pages == null)
        {
            Debug.LogError("❌ BookContent.m_Pages 데이터를 가져오지 못했습니다.");
            return;
        }

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

        UpdateCompletionText(); // 퍼센트 표시 초기화
    }

    public void UpdateCompletionText()
    {
        if (completionText == null || ArtifactStatusManager.Instance == null)
        {
            Debug.LogWarning("⚠️ completionText 또는 ArtifactStatusManager가 존재하지 않음");
            return;
        }

        int total = artifactObjects.Length;
        int found = 0;

        foreach (var obj in artifactObjects)
        {
            if (ArtifactStatusManager.Instance.IsFound(obj.name))
                found++;
        }

        float percent = (total == 0) ? 0f : (float)found / total * 100f;
        completionText.text = $"Complete:{percent:F1}%";
    }
}
