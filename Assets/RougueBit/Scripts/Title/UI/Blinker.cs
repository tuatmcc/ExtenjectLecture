using DG.Tweening;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class Blinker : MonoBehaviour
{
    private TextMeshProUGUI text;
    [SerializeField] private float duration = 1.0f;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        text.DOFade(0, duration).SetLoops(-1, LoopType.Yoyo);
    }
}
