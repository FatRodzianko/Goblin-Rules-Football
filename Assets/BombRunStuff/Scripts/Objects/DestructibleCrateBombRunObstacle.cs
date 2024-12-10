using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleCrateBombRunObstacle : BaseBombRunObstacle
{
    [SerializeField] private Animator _animator;
    public override void DamageObstacle(int damageAmount)
    {
        _animator.Play("DestrucibleCrateExplosion");
    }
    public void ExplosionComplete()
    {
        this.DestroyObstacle();
    }
}
