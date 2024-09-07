using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnit : MonoBehaviour
{
    private Vector3 _targetPosition;
    private float _moveSpeed = 4f;
    private float _stoppingDistance = 0.05f;

    

    private void Awake()
    {
        _targetPosition = this.transform.position;
    }
    private void Update()
    {
        if (Vector2.Distance(transform.position, _targetPosition) > _stoppingDistance)
        {
            Vector3 moveDirection = (_targetPosition - this.transform.position).normalized;
            transform.position += moveDirection * _moveSpeed * Time.deltaTime;
        }
    }
    public void Move(Vector3 targetPosition)
    {
        _targetPosition = targetPosition;
    }
}
