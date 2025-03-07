using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("Prefabs")]
    [SerializeField] private Transform _shootProjectilePrefab;

    [Header("Shooting")]
    [SerializeField] private Vector3 _shootPoint;

    private void Awake()
    {
        if (TryGetComponent<MoveAction>(out MoveAction moveAction))
        {
            moveAction.OnStartMoving += MoveAction_OnStartMoving;
            moveAction.OnStopMoving += MoveAction_OnStopMoving;
        }
        if (TryGetComponent<ShootAction>(out ShootAction shootAction))
        {
            shootAction.OnStartShooting += ShootAction_OnStartShooting;
            shootAction.OnStopShooting += ShootAction_OnStopShooting;
        }
    }

    

    private void OnDisable()
    {
        if (TryGetComponent<MoveAction>(out MoveAction moveAction))
        {
            moveAction.OnStartMoving -= MoveAction_OnStartMoving;
            moveAction.OnStopMoving -= MoveAction_OnStopMoving;
        }
        if (TryGetComponent<ShootAction>(out ShootAction shootAction))
        {
            shootAction.OnStartShooting -= ShootAction_OnStartShooting;
            shootAction.OnStopShooting -= ShootAction_OnStopShooting;
        }
    }

    private void MoveAction_OnStartMoving(object sender, EventArgs e)
    {
        _animator.SetBool("IsWalking", true);
    }
    private void MoveAction_OnStopMoving(object sender, EventArgs e)
    {
        _animator.SetBool("IsWalking", false);
    }
    private void ShootAction_OnStopShooting(object sender, EventArgs e)
    {

    }

    private void ShootAction_OnStartShooting(object sender, ShootAction.OnStartShootingEventArgs e)
    {
        _animator.SetTrigger("Shoot");
        Transform shootProjectile = Instantiate(_shootProjectilePrefab, this.transform.position + _shootPoint, Quaternion.identity);
        ShootProjectile shootProjectileScript = shootProjectile.GetComponent<ShootProjectile>();

        shootProjectileScript.Setup(e.TargetUnit.GetWorldPosition());
    }

    public void FlipSprite(bool flipSprite)
    {
        if (_spriteRenderer.flipX != flipSprite)
        {
            _spriteRenderer.flipX = flipSprite;
            _shootPoint.x = _shootPoint.x * -1;
        }
            
    }
    public bool GetSpriteFlipX()
    {
        return _spriteRenderer.flipX;
    }
}
