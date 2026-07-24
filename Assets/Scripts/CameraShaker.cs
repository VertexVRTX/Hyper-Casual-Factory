using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance { get; private set; }

    private Vector3 _originalPosition;

    private void Awake()
    {
        Instance = this;
        _originalPosition = transform.localPosition;
    }

    public void ShakeOnWrong()
    {
        transform.DOKill(true);
        transform.localPosition = _originalPosition;

        transform.DOShakePosition(0.3f, strength: new Vector3(0.3f, 0.3f, 0f), vibrato: 25, randomness: 90)
                 .OnComplete(() => transform.localPosition = _originalPosition);
    }

    public void PunchOnCombo()
    {
        transform.DOKill(true);
        transform.localPosition = _originalPosition;

        transform.DOPunchPosition(new Vector3(0f, 0f, -0.4f), duration: 0.25f, vibrato: 5, elasticity: 0.5f)
                 .OnComplete(() => transform.localPosition = _originalPosition);
    }
}
