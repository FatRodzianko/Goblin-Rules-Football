using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootProjectile : MonoBehaviour
{
    // events
    public event EventHandler OnProjectileHitTarget;

    private Vector3 _moveDirection;
    private float _moveSpeed = 100f;
    private float _maxDistanceToTravel;
    private float _totalDistanceTraveled = 0f;
    [SerializeField] private float _yOffset;

    private ShootAction _shootAction;

    [Header("Target")]
    private Vector3 _targetPosition;

    [Header("VFX")]
    [SerializeField] private TrailRenderer _trailRenderer;
    [SerializeField] private Transform _bulletHitVFX;

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
            _trailRenderer.transform.parent = null;

            OnProjectileHitTarget?.Invoke(this, EventArgs.Empty);

            Instantiate(_bulletHitVFX, this.transform.position, Quaternion.identity);
            //_shootAction.ProjectileHitTarget();
            Debug.Log("ShootProjectile: will now destroy shoot projectile object " + Time.time.ToString("0.00000000"));
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
        //_shootAction = shootAction;
        _targetPosition = targetPosition;
        _targetPosition.y += _yOffset;
        _moveDirection = (_targetPosition - this.transform.position).normalized;
        _maxDistanceToTravel = Vector3.Distance(_targetPosition, this.transform.position);
    }
}
