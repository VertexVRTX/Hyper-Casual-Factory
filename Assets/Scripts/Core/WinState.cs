using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinState : IGameState
{
    private readonly GameManager _gm;
    public WinState(GameManager gm) { _gm = gm; }

    public void Enter()
    {
        _gm.SaveManager.TrySaveBest(_gm.Score.CurrentScore);
        _gm.UI.ShowWin(_gm.Score.CurrentScore, _gm.SaveManager.BestScore);
        _gm.OnGameFinished();
    }
    public void Tick() { }
    public void Exit() => _gm.UI.HideWin();
}
