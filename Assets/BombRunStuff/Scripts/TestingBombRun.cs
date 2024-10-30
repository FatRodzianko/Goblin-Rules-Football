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
            GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPositon(MouseWorld.GetPosition());
            GridPosition startGridPosition = new GridPosition(0, 0);

            
            List<GridPosition> pathGridPositionList = PathFinding.Instance.FindPath(startGridPosition, mouseGridPosition, out int pathLength);

            for (int i = 0, n = pathGridPositionList.Count - 1; i < n; i++)
            {
                Debug.DrawLine(
                    LevelGrid.Instance.GetWorldPosition(pathGridPositionList[i]),
                    LevelGrid.Instance.GetWorldPosition(pathGridPositionList[i + 1]),
                    Color.white,
                    10f
                    );
            }
        }
    }
}
