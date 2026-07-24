using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScreenFlashEffect : MonoBehaviour
{
    public static ScreenFlashEffect Instance { get; private set; }
    public Image flashImage;
    public float flashDuration = 0.3f;
    public float targetAlpha = 0.4f;

    private void Awake()
    {
        Instance = this;
        flashImage.color = new Color(flashImage.color.r, flashImage.color.g, flashImage.color.b, 0f);
    }

    public void TriggerFlash()
    {
        flashImage.DOFade(targetAlpha, flashDuration / 2).OnComplete(() =>
        {
            flashImage.DOFade(0f, flashDuration / 2);
        });
    }
}
