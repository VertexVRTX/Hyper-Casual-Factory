using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private TMP_Text textMesh;
    [SerializeField] private float moveDistance = 1.5f;
    [SerializeField] private float duration = 0.8f;

    public void Init(int amount, bool isBonus = false, bool isTimeBonus = false)
    {
        if (isTimeBonus)
        {
            textMesh.text = $"+{amount} sec";
            textMesh.color = new Color(0.2f, 0.8f, 1f);
        }
        else if (isBonus)
        {
            textMesh.text = $"+{amount} BONUS!";
            textMesh.color = Color.yellow;
        }
        else
        {
            textMesh.text = $"+{amount}";
            textMesh.color = Color.white;
        }

        Sequence seq = DOTween.Sequence();

        seq.Join(transform.DOMoveY(transform.position.y + moveDistance, duration).SetEase(Ease.OutCubic));
        seq.Join(textMesh.DOFade(0f, duration).SetEase(Ease.InQuad));

        seq.SetUpdate(true);

        seq.OnComplete(() => Destroy(gameObject));
    }
}
