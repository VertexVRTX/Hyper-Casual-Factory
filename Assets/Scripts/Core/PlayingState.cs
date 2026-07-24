using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayingState : IGameState
{
    private readonly GameManager _gm;
    public PlayingState(GameManager gm) { _gm = gm; }

    public void Enter()
    {
        _gm.Timer.StartTimer();
        _gm.Conveyor.StartSpawning();
    }

    public void Tick()
    {
        _gm.Timer.Tick(Time.deltaTime);
    }

    public void Exit()
    {
        _gm.Conveyor.StopSpawning();
    }
}
