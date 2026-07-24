using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateMachine
{
    public IGameState CurrentState { get; private set; }

    public void Initialize(IGameState startState)
    {
        CurrentState = startState;
        CurrentState.Enter();
    }

    public void ChangeState(IGameState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Tick()
    {
        CurrentState?.Tick();
    }
}
