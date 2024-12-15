using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunDoor : MonoBehaviour
{
    private GridPosition _gridPosition;
    [SerializeField] private bool _isOpen = false;

    [SerializeField] private Animator _animator;
    private void Start()
    {
        _gridPosition = LevelGrid.Instance.GetGridPositon(this.transform.position);
    }
    public void Interact()
    {
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
        PathFinding.Instance.SetIsWalkableGridPosition(_gridPosition, true);
        _animator.Play("Open");
    }
    private void CloseDoor()
    {
        _isOpen = false;
        PathFinding.Instance.SetIsWalkableGridPosition(_gridPosition, false);
        _animator.Play("Closed");
    }
    public bool GetIsOpen()
    {
        return _isOpen;
    }
}
