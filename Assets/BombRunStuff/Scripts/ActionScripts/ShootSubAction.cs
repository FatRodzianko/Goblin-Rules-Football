using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootSubAction : BaseSubAction
{
    [Header("Testing Stuff")]
    [SerializeField] private float _countDownLength;

    [Header("Parameters for Parent Action")]
    [SerializeField] private BodyPart _bodyPartToShoot;
    private float _timer;

    private void Update()
    {
        if (!_isActive)
            return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            SubActionComplete();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            SubActionCancelled();
        }
    }
    public override void TakeSubAction(GridPosition gridPosition, Action onSubActionComplete)
    {
        base.TakeSubAction(gridPosition, onSubActionComplete);
        _timer = _countDownLength;
        SubActionStart(onSubActionComplete);
    }

    public override void TakeActionFromParentAction()
    {
        ShootAction shootAction = _parentAction as ShootAction;
        shootAction.TakeActionFromSubAction(_gridPosition, _onSubActionComplete);
    }
}
