using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnitAnimator_PassThrough : MonoBehaviour
{
    [SerializeField] private BombRunUnitAnimator _bombRunUnitAnimator;

    private void Awake()
    {
        if (_bombRunUnitAnimator == null)
        {
            _bombRunUnitAnimator = this.transform.parent.GetComponent<BombRunUnitAnimator>();
        }
    }

    public void FireShootProjectile()
    {
        _bombRunUnitAnimator.FireShootProjectile();
    }
}
