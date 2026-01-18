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

    [Header("SubAction GUI and Stuff")]
    [SerializeField] private Transform _shootSubActionGUIPrefab;
    [SerializeField] private Transform _shootSubActionGUIObject;
    [SerializeField] private ShootSubActionUI _shootSubActionGUIScript;

    private void Update()
    {
        if (!_isActive)
            return;

        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    SubActionComplete();
        //}
        //else if (Input.GetKeyDown(KeyCode.C))
        //{
        //    SubActionCancelled();
        //}
        if (InputManagerBombRun.Instance.IsRightMouseButtonDownThisFrame())
        {
            SubActionCancelled();
        }
    }
    public override void TakeSubAction(GridPosition gridPosition, Action onSubActionComplete)
    {
        base.TakeSubAction(gridPosition, onSubActionComplete);
        _timer = _countDownLength;
        SubActionStart(onSubActionComplete);
        SpawnShootSubActionGUI();

        _unit.SetActionDirection(LevelGrid.Instance.GetWorldPosition(gridPosition) - LevelGrid.Instance.GetWorldPosition(_unit.GetGridPosition()));
    }

    public override void TakeActionFromParentAction()
    {
        //ShootAction shootAction = _parentAction as ShootAction;
        //shootAction.TakeActionFromSubAction(_gridPosition, _onSubActionComplete, _bodyPartToShoot);
        _parentAction.TakeAction(_gridPosition, _onSubActionComplete, _bodyPartToShoot);
    }
    void SpawnShootSubActionGUI()
    {
        _shootSubActionGUIObject = Instantiate(_shootSubActionGUIPrefab, this.transform);

        Vector3 localPosition = _shootSubActionGUIObject.transform.localPosition;
        Vector3 targetPosition = LevelGrid.Instance.GetUnitAtGridPosition(this._gridPosition).transform.position;

        // add check to see when the object is off screen, and if so, flip the x offset it spawns at? If target is too far left, spawn UI object on right side of target instead of the left

        _shootSubActionGUIObject.transform.position = new Vector3(targetPosition.x + localPosition.x, targetPosition.y + localPosition.y, targetPosition.z);
        _shootSubActionGUIScript = _shootSubActionGUIObject.GetComponent<ShootSubActionUI>();
        _shootSubActionGUIScript.InitializeShootSubActionUI(this);
        _shootSubActionGUIScript.SetPlayerSelectionCallback(PlayerSelectedBodyPart);
    }

    public override void DestroySpawnedObjects()
    {
        Destroy(_shootSubActionGUIObject.gameObject);
    }
    public void PlayerSelectedBodyPart(BodyPart bodyPart)
    {
        Debug.Log("ShootSubAction: PlayerSelectedBodyPart: " + bodyPart);
        _bodyPartToShoot = bodyPart;
        SubActionComplete();
    }
}
