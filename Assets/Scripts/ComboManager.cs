using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    public int hitsForNextMultiplier = 3;
    public int maxMultiplier = 5;

    public int Multiplier { get; private set; } = 1;
    private int _currentProgress;

    public event Action<int, int> OnComboChanged;
    public event Action<int> OnMultiplierIncreased;

    public void AddHit()
    {
        _currentProgress++;

        if (_currentProgress >= hitsForNextMultiplier)
        {
            if (Multiplier < maxMultiplier)
            {
                _currentProgress = 0;
                Multiplier++;
                OnMultiplierIncreased?.Invoke(Multiplier);

                if (CameraShaker.Instance != null)
                {
                    CameraShaker.Instance.PunchOnCombo();
                }
            }
            else
            {
                _currentProgress = hitsForNextMultiplier;

            }
        }

        OnComboChanged?.Invoke(_currentProgress, Multiplier);
    }

    public void ResetCombo()
    {
        _currentProgress = 0;
        Multiplier = 1;
        OnComboChanged?.Invoke(_currentProgress, Multiplier);
    }
}
