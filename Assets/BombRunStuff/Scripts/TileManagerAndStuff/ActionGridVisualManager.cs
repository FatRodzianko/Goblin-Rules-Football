using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum GridVisualType
{
    White,
    Blue,
    Red,
    Yellow,
    RedSoft
}

public class ActionGridVisualManager : MonoBehaviour
{
    [Serializable]
    public struct GridVisualTypeColor
    {
        public GridVisualType gridVisualType;
        public Color color;
        public Tile tile;
    }
    
    public static ActionGridVisualManager Instance { get; private set; }


    [Header("Tilemaps")]
    [SerializeField] private Tilemap _actionVisualsTileMap;


    [Header("Grid Visual Tiles")]
    [SerializeField] private Tile _actionVisualTile;
    [SerializeField] private bool _calculatingVisualGrid = false;

    [Header("Tile List")]
    [SerializeField] private List<GridPosition> _actionVisualPositions = new List<GridPosition>();

    [Header("Grid System Stuff")]
    [SerializeField] private GridSystem<GridObject> _gridSystem;

    [SerializeField] private List<GridVisualTypeColor> _gridVisualTypeColorList;

    private void Awake()
    {
        MakeInstance();
    }
    void MakeInstance()
    {
        if (Instance != null)
        {
            Debug.Log("MakeInstance: more than one ActionGridVisualManager. Destroying...");
            Destroy(this);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        // event subscriptions
        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged;
        UnitActionSystem.Instance.OnBusyChanged += UnitActionSystem_OnBusyChanged;

        //// cache the grid system?
        //_gridSystem = LevelGrid.Instance.GetGridObjectGridSystem();

        //// Update the action visuals for the initially selected unit and selected action
        //UpdateActionVisuals();
    }

    

    private void OnDisable()
    {
        // event subscriptions
        UnitActionSystem.Instance.OnSelectedActionChanged -= UnitActionSystem_OnSelectedActionChanged;
        UnitActionSystem.Instance.OnBusyChanged -= UnitActionSystem_OnBusyChanged;
    }
    public void InitializeActionGridVisualManager()
    {
        // cache the grid system?
        _gridSystem = LevelGrid.Instance.GetGridObjectGridSystem();

        // Update the action visuals for the initially selected unit and selected action
        //UpdateActionVisuals();
    }


    public void AddActionVisualToGridPosition(GridPosition gridPosition, Tile gridVisual, Color color)
    {
        gridVisual.color = color;
        _actionVisualsTileMap.SetTile(new Vector3Int(gridPosition.x, gridPosition.y, 0), null);
        _actionVisualsTileMap.SetTile(new Vector3Int(gridPosition.x, gridPosition.y, 0), gridVisual);
        _actionVisualPositions.Add(gridPosition);
    }
    public void RemoveActionVisualToGridPosition(GridPosition gridPosition)
    {
        _actionVisualsTileMap.SetTile(new Vector3Int(gridPosition.x, gridPosition.y, 0), null);
    }
    private void ShowGridPositionRange(GridPosition gridPosition, int range, GridVisualType gridVisualType)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                GridPosition testGridPosition = gridPosition + new GridPosition(x, y);
                if (testGridPosition == gridPosition)
                {
                    continue;
                }
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }
                int testDistance = Mathf.Abs(x) + Mathf.Abs(y);
                if (testDistance > range)
                {
                    continue;
                }
                
                gridPositionList.Add(testGridPosition);
            }
        }
        ShowActionVisualsFromList(gridPositionList, gridVisualType);
    }
    private void ShowGridPositionRangeSquare(GridPosition gridPosition, int range, GridVisualType gridVisualType)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                GridPosition testGridPosition = gridPosition + new GridPosition(x, y);
                if (testGridPosition == gridPosition)
                {
                    continue;
                }
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }
                gridPositionList.Add(testGridPosition);
            }
        }
        ShowActionVisualsFromList(gridPositionList, gridVisualType);
    }
    private void ShowGridPositionRangeRadius(GridPosition gridPosition, int range, GridVisualType gridVisualType)
    {
        List<GridPosition> gridPositionList = LevelGrid.Instance.GetGridPositionsInRadius(gridPosition, range);
        ShowActionVisualsFromList(gridPositionList, gridVisualType);
    }
    public void ShowActionVisualsFromList(List<GridPosition> gridPositions, GridVisualType gridVisualType)
    {
        if (gridPositions.Count == 0)
            return;

        foreach (GridPosition gridPosition in gridPositions)
        {
            AddActionVisualToGridPosition(gridPosition, GetGridVisualTypeTile(gridVisualType), GetGridVisualTypeColor(gridVisualType));
        }
    }
    public void HideAllActionVisuals()
    {
        foreach (GridPosition gridPosition in _actionVisualPositions)
        {
            RemoveActionVisualToGridPosition(gridPosition);
        }
        _actionVisualPositions.Clear();
    }
    private void UnitActionSystem_OnSelectedActionChanged(object sender, EventArgs e)
    {
        UpdateActionVisuals();
    }
    private void UnitActionSystem_OnBusyChanged(object sender, bool busy)
    {
        if (busy)
        {
            return;
        }
        UpdateActionVisuals();
    }
    private async void UpdateActionVisuals()
    {
        if (_calculatingVisualGrid)
            return;
        //HideAllActionVisuals();

        BombRunUnit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();
        //Debug.Log("UpdateActionVisuals: " + selectedAction);

        if (selectedAction == null)
        {
            Debug.Log("UpdateActionVisuals: selected action is null?");
            HideAllActionVisuals();
            return;
        }

        GridVisualType gridVisualType;
        GridVisualType gridRangeVisualType = GridVisualType.RedSoft;
        int gridVisualRange = -1;
        bool squareGridRange = false;

        switch (selectedAction)
        {
            default:
            case MoveAction moveAction:
                gridVisualType = GridVisualType.White;
                break;
            case SpinAction spinAction:
                gridVisualType = GridVisualType.Blue;
                break;
            case DefendAction defendAction:
                gridVisualType = GridVisualType.Blue;
                break;
            case GrenadeAction grenadeAction:
                gridVisualType = GridVisualType.Yellow;
                break;
            case SwordAction swordAction:
                gridVisualType = GridVisualType.Red;
                gridVisualRange = swordAction.GetMaxSwordDistance();
                gridRangeVisualType = GridVisualType.RedSoft;
                if (LevelGrid.Instance.GetGridObjectGridSystem().GetType() == typeof(GridSystemHex<GridObject>))
                {
                    //ShowGridPositionRangeRadius(selectedUnit.GetGridPosition(), swordAction.GetMaxSwordDistance(), GridVisualType.RedSoft);

                }
                else
                {
                    //ShowGridPositionRangeSquare(selectedUnit.GetGridPosition(), swordAction.GetMaxSwordDistance(), GridVisualType.RedSoft);
                    squareGridRange = true;                    
                }
                
                break;
            case InteractAction interactAction:
                gridVisualType = GridVisualType.Blue;
                break;
            case ShootAction shootAction:
                gridVisualType = GridVisualType.Red;
                //ShowGridPositionRangeRadius(selectedUnit.GetGridPosition(), shootAction.GetMaxShootDistance(), GridVisualType.RedSoft);
                gridVisualRange = shootAction.GetMaxShootDistance();
                gridRangeVisualType = GridVisualType.RedSoft;
                break;
        }

        // trying to use tasks for multithreading...
        List<GridPosition> actionVisualPositions = new List<GridPosition>();
        if (selectedAction.CanGetValidListAsTask())
        {
            _calculatingVisualGrid = true;
            //Debug.Log("UpdateActionVisuals: Getting valid list as a task...");
            actionVisualPositions = await selectedAction.GetValidActionGridPositionListAsTask();
        }
        else
        {
            actionVisualPositions = selectedAction.GetValidActionGridPositionList();
        }
        // trying to use tasks for multithreading...

        //List<GridPosition> actionVisualPositions = selectedAction.GetValidActionGridPositionList();

        // Hide the current action visuals, then add the new ones
        HideAllActionVisuals();
        // Add the semi-transparent "Grid Range" visual indicators, if the action uses one?
        if (gridVisualRange > 0)
        {
            if (squareGridRange)
            {
                ShowGridPositionRangeSquare(selectedUnit.GetGridPosition(), gridVisualRange, gridRangeVisualType);
            }
            else
            {
                ShowGridPositionRangeRadius(selectedUnit.GetGridPosition(), gridVisualRange, gridRangeVisualType);
            }
        }
        ShowActionVisualsFromList(actionVisualPositions, gridVisualType);
        _calculatingVisualGrid = false;
    }
    private Color GetGridVisualTypeColor(GridVisualType gridVisualType)
    {
        foreach (GridVisualTypeColor gridVisualTypeColor in _gridVisualTypeColorList)
        {
            if (gridVisualTypeColor.gridVisualType == gridVisualType)
            {
                return gridVisualTypeColor.color;
            }
        }
        Debug.LogError("Could not find GetGridVisualTypeColor for GridVisualType: " + gridVisualType);
        return Color.white;
    }
    private Tile GetGridVisualTypeTile(GridVisualType gridVisualType)
    {
        foreach (GridVisualTypeColor gridVisualTypeColor in _gridVisualTypeColorList)
        {
            if (gridVisualTypeColor.gridVisualType == gridVisualType)
            {
                return gridVisualTypeColor.tile;
            }
        }
        Debug.LogError("Could not find GetGridVisualTypeTile for GridVisualType: " + gridVisualType);
        return null;
    }
    public bool CalculatingVisualGrid()
    {
        return _calculatingVisualGrid;
    }
}
