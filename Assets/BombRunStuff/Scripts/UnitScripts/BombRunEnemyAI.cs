using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BombRunEnemyAI : MonoBehaviour
{
    private enum State
    {
        WaitingForEnemyTurn,
        TakingTurn,
        Busy,
    }
    private State _state;
    private float _timer;
    private void Awake()
    {
        _state = State.WaitingForEnemyTurn;
    }
    private void Start()
    {
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
    }

    private void Update()
    {
        if (TurnSystem.Instance.IsPlayerTurn())
            return;

        switch (_state) 
        {
            case State.WaitingForEnemyTurn:
                break;
            case State.TakingTurn:
                TakingturnTimer();
                break;
            case State.Busy:
                break;
        }

        //_timer -= Time.deltaTime;
        //if (_timer <= 0f)
        //{
        //    TurnSystem.Instance.NextTurn();
        //}
    }
    private void TakingturnTimer()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _state = State.Busy;
            if (TryTakeEnemyAIAction(SetStateTakingTurn))
            {
                _state = State.Busy;
            }
            else
            {
                // no more enemy units have actions they can take. End enemy turn
                TurnSystem.Instance.NextTurn();
            }
        }
    }
    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if (!TurnSystem.Instance.IsPlayerTurn())
        {
            _state = State.TakingTurn;
            _timer = 2f;
        }        
    }
    private void SetStateTakingTurn()
    {
        _timer = 0.5f;
        _state = State.TakingTurn;
    }
    private bool TryTakeEnemyAIAction(Action onEnemyActionComplete)
    {
        Debug.Log("TakeEnemyAIAction: ");
        foreach (BombRunUnit enemyUnit in BombRunUnitManager.Instance.GetEnemyUnitList())
        {
            if (TryTakeEnemyAIAction(enemyUnit, onEnemyActionComplete))
            {
                return true;
            }
        }
        return false;
    }
    private bool TryTakeEnemyAIAction(BombRunUnit enemyUnit, Action onEnemyActionComplete)
    {
        BombRunEnemyAIAction bestEnemyAIAction = null;
        BaseAction bestBaseAction = null;
        foreach (BaseAction baseAction in enemyUnit.GetBaseActionArray())
        {
            if (!enemyUnit.CanSpendActionPointsToTakeAction(baseAction))
            {
                // enemy cannot afford action
                continue;
            }
            
            if (bestEnemyAIAction == null)
            {
                bestEnemyAIAction = baseAction.GetBestEnemyAIAction();
                bestBaseAction = baseAction;
            }
            else
            {
                BombRunEnemyAIAction testEnemyAIAction = baseAction.GetBestEnemyAIAction();
                if (testEnemyAIAction != null && testEnemyAIAction._ActionValue > bestEnemyAIAction._ActionValue)
                {
                    bestEnemyAIAction = testEnemyAIAction;
                    bestBaseAction = baseAction;
                }
            }
        }
        if (bestEnemyAIAction != null && enemyUnit.TrySpendActionPointsToTakeAction(bestBaseAction, bestEnemyAIAction._GridPosition))
        {
            // add if statement for checking for sub action? If yes, then call TakeActionFromSubAction with the bodypart provided
            // // in future should try and add a "generic" subaction parameter? So sub actions can ask for things other than just target body parts?
            if (bestBaseAction.GetHasSubAction())
            {
                Debug.Log("TryTakeEnemyAIAction: action: " + bestBaseAction.name + " has a sub action. Targeting body part: " + bestEnemyAIAction._TargetBodyPart);
                bestBaseAction.TakeAction(bestEnemyAIAction._GridPosition, onEnemyActionComplete, bestEnemyAIAction._TargetBodyPart);
            }
            else
            {
                bestBaseAction.TakeAction(bestEnemyAIAction._GridPosition, onEnemyActionComplete);
            }
            

            return true;
        }
        else
        {
            // could not take an action
            return false;
        }
    }
}
