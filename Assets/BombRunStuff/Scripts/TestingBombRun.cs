using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingBombRun : MonoBehaviour
{
    [SerializeField] private BombRunUnit _unit;
    private void Start()
    {

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            _unit.GetMoveAction().GetValidActionGridPositionList();
        }
    }
}
