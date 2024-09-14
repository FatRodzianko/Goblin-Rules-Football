using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TestingBombRun : MonoBehaviour
{
    [SerializeField] private BombRunUnit _unit;
    [SerializeField] private Tile _actionVisualTile;
    private void Start()
    {

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            //List<GridPosition> movePositions = _unit.GetMoveAction().GetValidActionGridPositionList();
            //List<GridPosition> movePositions = UnitActionSystem.Instance.GetSelectedUnit().GetMoveAction().GetValidActionGridPositionList();
            //BombRunTileMapManager.Instance.HideAllActionVisuals();
            //BombRunTileMapManager.Instance.ShowActionVisualsFromList(movePositions, _actionVisualTile, Color.white);
        }
    }
}
