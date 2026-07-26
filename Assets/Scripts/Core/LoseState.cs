using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseState : IGameState
{
    private readonly GameManager _gm;
    public LoseState(GameManager gm) { _gm = gm; }

    public void Enter()
    {
        _gm.SaveManager.TrySaveBest(_gm.Score.CurrentScore);
        _gm.UI.ShowLose(_gm.Score.CurrentScore, _gm.SaveManager.BestScore);
        _gm.OnGameFinished();
    }
    public void Tick() { }
    public void Exit() => _gm.UI.HideLose();
}
