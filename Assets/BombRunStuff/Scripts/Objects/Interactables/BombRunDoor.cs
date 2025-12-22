using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunDoor : BaseBombRunObstacle, IInteractable
{
    private GridPosition _gridPosition;
    [SerializeField] private bool _isOpen = false;

    [SerializeField] private Animator _animator;

    private Action _onInteractionComplete;
    private bool _isActive;
    private float _timer;

    protected override void Start()
    {
        base.Start();
        _gridPosition = LevelGrid.Instance.GetGridPositon(this.transform.position);
    }
    private void Update()
    {
        if (!_isActive)
        {
            return;
        }

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _isActive = false;
            _onInteractionComplete();
        }
    }
    public void Interact(Action onInteractionComplete)
    {
        this._onInteractionComplete = onInteractionComplete;
        _isActive = true;
        _timer = 0.5f;

        Debug.Log("BombRunDoor: Interact");

        if (_isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }

    }
    private void OpenDoor()
    {
        _isOpen = true;
        this.SetIsWalkable(true);
        PathFinding.Instance.SetIsWalkableGridPosition(_gridPosition, true);
        SetObstacleCoverType(ObstacleCoverType.None);
        _animator.Play("Open");
    }
    private void CloseDoor()
    {
        _isOpen = false;
        this.SetIsWalkable(false);
        PathFinding.Instance.SetIsWalkableGridPosition(_gridPosition, false);
        SetObstacleCoverType(ObstacleCoverType.Full);
        _animator.Play("Closed");
    }
    public bool GetIsOpen()
    {
        return _isOpen;
    }
}
