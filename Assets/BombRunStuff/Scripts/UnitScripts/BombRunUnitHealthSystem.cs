using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnitHealthSystem : MonoBehaviour
{
    // events
    public event EventHandler OnDead;

    [SerializeField] private int _startingHealth = 100;
    [SerializeField] private int _health = 100;

    private void Awake()
    {
        _health = _startingHealth;
    }
    public void TakeDamage(int damageAmount)
    {
        _health -= damageAmount;

        if (_health <= 0)
        {
            _health = 0;
            Die();
        }
    }
    private void Die()
    {
        OnDead?.Invoke(this, EventArgs.Empty);
    }
}
