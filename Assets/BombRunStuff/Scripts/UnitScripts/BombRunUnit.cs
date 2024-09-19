using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnit : MonoBehaviour
{

    [Header("GridPosition stuff")]
    private GridPosition _gridPosition;

    [Header("Actions")]
    [SerializeField] MoveAction _moveAction;
    [SerializeField] SpinAction _spinAction;
    [SerializeField] private BaseAction[] _baseActionArray;

    private void Awake()
    {
        _moveAction = GetComponent<MoveAction>();
        _spinAction = GetComponent<SpinAction>();
        _baseActionArray = GetComponents<BaseAction>();
    }
    private void Start()
    {
        _gridPosition = LevelGrid.Instance.GetGridPositon(this.transform.position);
        LevelGrid.Instance.AddUnitAtGridPosition(_gridPosition, this);
    }
    private void Update()
    {
        GridPosition newGridPosition = LevelGrid.Instance.GetGridPositon(this.transform.position);
        if (newGridPosition != _gridPosition)
        {
            // unit changed grid position
            LevelGrid.Instance.UnitMovedGridPosition(this, _gridPosition, newGridPosition);
            _gridPosition = newGridPosition;
        }
    }
    public MoveAction GetMoveAction()
    {
        return _moveAction;
    }
    public SpinAction GetSpinAction()
    {
        return _spinAction;
    }
    public GridPosition GetGridPosition()
    {
        return _gridPosition;
    }
    public BaseAction[] GetBaseActionArray()
    {
        return _baseActionArray;
    }
}
