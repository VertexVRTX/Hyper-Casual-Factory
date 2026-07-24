using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuState : IGameState
{
    private readonly GameManager _gm;
    public MenuState(GameManager gm) { _gm = gm; }

    public void Enter() => _gm.UI.ShowMenu();
    public void Tick() { }
    public void Exit() => _gm.UI.HideMenu();
}
