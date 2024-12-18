using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractAction : BaseAction
{
    private int maxInteractDistance = 1;
    private void Update()
    {
        if (!_isActive)
            return;
    }
    public override string GetActionName()
    {
        return "Interact";
    }

    public override BombRunEnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new BombRunEnemyAIAction
        {
            _GridPosition = gridPosition,
            _ActionValue = 0

        };
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = _unit.GetGridPosition();

        // this should just be checking for immediate neighbor positions that are interactable
        // if I ever make it so neighbor nodes are cached I might just get the neighbors from there?
        for (int x = -maxInteractDistance; x <= maxInteractDistance; x++)
        {
            for (int z = -maxInteractDistance; z <= maxInteractDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }
                // check if an obstacle exists at this point. All interactables will be obstacles?
                // should separate later in own "interactables" since an obstacle can be an interactable (like a door) but an interactable may not be an obstacle (like a switch on a wall
                if (!LevelGrid.Instance.HasAnyObstacleOnGridPosition(testGridPosition))
                {
                    continue;
                }
                BaseBombRunObstacle obstacle = LevelGrid.Instance.GetObstacleAtGridPosition(testGridPosition);
                bool isInteractable = obstacle.IsInteractable();
                Debug.Log("InteractAction: GetValidActionGridPositionList: Obstacle: " + obstacle.name + " at grid position: " + testGridPosition.ToString() + " interactable: " + isInteractable.ToString());
                if (!isInteractable)
                {
                    continue;
                }


                validGridPositionList.Add(testGridPosition);
            }
        }
        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        Debug.Log("TakeAction: Interact");

        IInteractable interactable = LevelGrid.Instance.GetInteractableAtGridPosition(gridPosition);
        interactable.Interact(OnInteractComplete);

        ActionStart(onActionComplete);
    }
    private void OnInteractComplete()
    {
        ActionComplete();
    }
}
