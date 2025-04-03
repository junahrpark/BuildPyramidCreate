using UnityEngine;
using UnityEngine.UI;

public class NeedleRotator : MonoBehaviour
{
    public float rotationSpeed = 30f; // 회전 속도 (1초에 180도)

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        rectTransform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
    }
}
