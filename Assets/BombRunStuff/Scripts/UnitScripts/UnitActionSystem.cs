using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitActionSystem : MonoBehaviour
{
    [SerializeField] private BombRunUnit _selectedUnit;
    [SerializeField] private LayerMask _unitLayerMask;

    private void Update()
    {
        
        if (InputManagerBombRun.Instance.IsMouseButtonDownThisFrame())
        {
            if (TryHandleUnitSelection())
                return;

            _selectedUnit.Move(MouseWorld.GetPosition());
        }
    }
    private bool TryHandleUnitSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(InputManagerBombRun.Instance.GetMouseScreenPosition());
        RaycastHit2D raycastHit = Physics2D.Raycast(ray.origin, ray.direction, float.MaxValue, _unitLayerMask);
        if (raycastHit)
        {
            if (raycastHit.transform.TryGetComponent<BombRunUnit>(out BombRunUnit unit))
            {
                _selectedUnit = unit;
                return true;
            }
        }
        return false;
    }
}
