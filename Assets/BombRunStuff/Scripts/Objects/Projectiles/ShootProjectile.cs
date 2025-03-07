using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootProjectile : MonoBehaviour
{
    private Vector3 _targetPosition;
    private Vector3 _moveDirection;
    private float _moveSpeed = 100f;
    private float _maxDistanceToTravel;
    private float _totalDistanceTraveled = 0f;
    [SerializeField] private float _yOffset;

    private void Update()
    {
        Vector3 currentPosition = this.transform.position;
        Vector3 newPosition = currentPosition + (_moveDirection * _moveSpeed * Time.deltaTime);
        float newDistance = _totalDistanceTraveled + Vector3.Distance(currentPosition, newPosition);

        if (newDistance >= _maxDistanceToTravel)
        {
            //newPosition = currentPosition + (_moveDirection * Vector3.Distance(_targetPosition, this.transform.position));
            newPosition = currentPosition + (_moveDirection * (_maxDistanceToTravel - _totalDistanceTraveled));
            this.transform.position = newPosition;
            Destroy(this.gameObject);
        }
        else
        {
            this.transform.position = newPosition;
        }
        _totalDistanceTraveled = newDistance;
    }
    public void Setup(Vector3 targetPosition)
    {
        _targetPosition = targetPosition;
        _targetPosition.y += _yOffset;
        _moveDirection = (_targetPosition - this.transform.position).normalized;
        _maxDistanceToTravel = Vector3.Distance(_targetPosition, this.transform.position);
    }
}
