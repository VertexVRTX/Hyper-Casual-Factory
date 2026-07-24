using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public float startTime = 60f;
    public float TimeLeft { get; private set; }
    public event Action<float> OnTimeChanged;
    public event Action OnTimeUp;

    public void ResetTimer() => TimeLeft = startTime;
    public void StartTimer() => TimeLeft = startTime;

    public void AddExtraTime(float extraSeconds)
    {
        if (TimeLeft <= 0) return;

        TimeLeft += extraSeconds;
        OnTimeChanged?.Invoke(TimeLeft);
    }

    public void Tick(float delta)
    {
        if (TimeLeft <= 0) return;
        TimeLeft -= delta;
        OnTimeChanged?.Invoke(TimeLeft);
        if (TimeLeft <= 0)
        {
            TimeLeft = 0;
            OnTimeUp?.Invoke();
            GameManager.Instance.OnTimerEnded();
        }
    }
}
