using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnitAnimator : MonoBehaviour
{
    [SerializeField] private BombRunUnit _unit;
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteMask _spriteMask;

    [Header("Prefabs")]
    [SerializeField] private Transform _shootProjectilePrefab;

    [Header("Shooting")]
    [SerializeField] private Vector3 _shootPoint;

    private void Awake()
    {
        if (_unit == null)
        {
            _unit = this.transform.parent.GetComponent<BombRunUnit>();
        }
        if (transform.parent.TryGetComponent<MoveAction>(out MoveAction moveAction))
        {
            moveAction.OnStartMoving += MoveAction_OnStartMoving;
            moveAction.OnStopMoving += MoveAction_OnStopMoving;
        }
        if (transform.parent.TryGetComponent<ShootAction>(out ShootAction shootAction))
        {
            shootAction.OnStartShooting += ShootAction_OnStartShooting;
            shootAction.OnStopShooting += ShootAction_OnStopShooting;
        }
        _unit.OnUnitVisibilityChanged += Unit_OnUnitVisibilityChanged;
        _unit.OnActionDirectionChanged += Unit_OnActionDirectionChanged;
        _spriteRenderer.RegisterSpriteChangeCallback(UnitSpriteChanged);
    }

    

    private void UnitSpriteChanged(SpriteRenderer spriteRenderer)
    {
        _spriteMask.sprite = spriteRenderer.sprite;
    }


    private void OnDisable()
    {
        if (transform.parent.TryGetComponent<MoveAction>(out MoveAction moveAction))
        {
            moveAction.OnStartMoving -= MoveAction_OnStartMoving;
            moveAction.OnStopMoving -= MoveAction_OnStopMoving;
        }
        if (transform.parent.TryGetComponent<ShootAction>(out ShootAction shootAction))
        {
            shootAction.OnStartShooting -= ShootAction_OnStartShooting;
            shootAction.OnStopShooting -= ShootAction_OnStopShooting;
        }
        _unit.OnUnitVisibilityChanged -= Unit_OnUnitVisibilityChanged;
        _unit.OnActionDirectionChanged -= Unit_OnActionDirectionChanged;
    }

    private void MoveAction_OnStartMoving(object sender, EventArgs e)
    {
        Debug.Log("MoveAction_OnStartMoving: " + _unit.name);
        _animator.SetBool("IsWalking", true);
    }
    private void MoveAction_OnStopMoving(object sender, EventArgs e)
    {
        _animator.SetBool("IsWalking", false);
    }
    private void ShootAction_OnStopShooting(object sender, EventArgs e)
    {
        
    }
    public void FireShootProjectile()
    {
        Debug.Log("FireShootProjectile: " + _unit.name);
        ShootAction shootAction = _unit.GetAction<ShootAction>();

        if (shootAction == null)
            return;

        Transform shootProjectile = Instantiate(_shootProjectilePrefab, this.transform.position + _shootPoint, Quaternion.identity);
        ShootProjectile shootProjectileScript = shootProjectile.GetComponent<ShootProjectile>();

        shootProjectileScript.OnProjectileHitTarget += ShootProjectile_OnProjectileHitTarget;

        //shootProjectileScript.Setup(shootAction.TargetUnit.GetWorldPosition());
        shootProjectileScript.Setup(shootAction.GetTargetUnitWorldPosition());
    }

    private void ShootProjectile_OnProjectileHitTarget(object sender, EventArgs e)
    {
        ShootProjectile shootProjectileScript = sender as ShootProjectile;
        ShootAction shootAction = _unit.GetAction<ShootAction>();
        if (shootAction == null)
            return;
        shootAction.ProjectileHitTarget();

        shootProjectileScript.OnProjectileHitTarget -= ShootProjectile_OnProjectileHitTarget;
        Debug.Log("ShootProjectile_OnProjectileHitTarget: unsubscribe shoot projectile event " + Time.time.ToString("0.00000000"));
    }

    private void ShootAction_OnStartShooting(object sender, ShootAction.OnStartShootingEventArgs e)
    {
        Debug.Log("ShootAction_OnStartShooting: " + _unit.name);
        _animator.SetTrigger("Shoot");
    }
    private void Unit_OnActionDirectionChanged(object sender, Vector2 e)
    {
        if (_unit.GetActionDirection().x < 0)
        {
            FlipSprite(true);
        }
        else
        {
            FlipSprite(false);
        }
    }
    public void FlipSprite(bool flipSprite)
    {
        //if (_spriteRenderer.flipX != flipSprite)
        //{
        //    _spriteRenderer.flipX = flipSprite;
        //    _shootPoint.x = _shootPoint.x * -1;
        //}
        Vector3 currentScale = this.transform.localScale;
        if ((currentScale.x < 0) != flipSprite)
        {
            //_spriteRenderer.flipX = flipSprite;

            currentScale.x *= -1;
            this.transform.localScale = currentScale;
            _shootPoint.x = _shootPoint.x * -1;
        }
    }
    public bool GetSpriteFlipX()
    {
        return (this.transform.localScale.x < 0);
        //return _spriteRenderer.flipX;
    }
    public void SetUnitVisibility(bool isVisible)
    {
        _spriteRenderer.enabled = isVisible;
        _spriteMask.enabled = isVisible;
    }
    public bool GetUnitVisibility()
    {
        return _spriteRenderer.enabled;
    }
    private void Unit_OnUnitVisibilityChanged(object sender, bool isVisible)
    {
        SetUnitVisibility(isVisible);
    }
    public Sprite GetCurrentSprite()
    {
        return _spriteRenderer.sprite;
    }
}
