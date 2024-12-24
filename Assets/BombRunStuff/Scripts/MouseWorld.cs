using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseWorld : MonoBehaviour
{
    public static MouseWorld instance;
    [SerializeField] private LayerMask _floorMaskLayer;

    [Header("Mouse Position")]
    [SerializeField] private GridPosition _mouseGridPosition;
    [SerializeField] private Vector3 _mouseWorldPosition;

    // events
    public event EventHandler<GridPosition> OnMouseGridPositionChange;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }
    private void Update()
    {
        UpdateMouseGridPosition();
    }
    public static Vector3 GetPosition()
    {
        //Ray ray = Camera.main.ScreenPointToRay(InputManagerBombRun.Instance.GetMouseScreenPosition());
        //RaycastHit2D raycastHit = Physics2D.Raycast(ray.origin, ray.direction, float.MaxValue, MouseWorld.instance._floorMaskLayer);
        //return raycastHit.point;

        return Camera.main.ScreenToWorldPoint(InputManagerBombRun.Instance.GetMouseScreenPosition());
    }
    private void UpdateMouseGridPosition()
    {
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(InputManagerBombRun.Instance.GetMouseScreenPosition());
        if (newPosition == _mouseWorldPosition)
            return;

        _mouseWorldPosition = newPosition;

        GridPosition newGridPosition = LevelGrid.Instance.GetGridPositon(_mouseWorldPosition);
        if (!LevelGrid.Instance.IsValidGridPosition(newGridPosition))
            return;
        if (newGridPosition == _mouseGridPosition)
            return;

        _mouseGridPosition = newGridPosition;
        OnMouseGridPositionChange?.Invoke(this, _mouseGridPosition);

    }
    public GridPosition GetCurrentMouseGridPosition()
    {
        return _mouseGridPosition;
    }

}
