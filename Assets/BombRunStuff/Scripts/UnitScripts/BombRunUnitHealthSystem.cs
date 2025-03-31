using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnitHealthSystem : MonoBehaviour
{
    // events
    public event EventHandler OnDead;
    public event EventHandler OnTakeDamage;

    [SerializeField] private int _startingHealth = 100;
    [SerializeField] private int _health = 100;
    [SerializeField] private int _maxHealth;

    private void Awake()
    {
        _health = _startingHealth;
        _maxHealth = _startingHealth;
    }
    public void TakeDamage(int damageAmount)
    {
        _health -= damageAmount;
        OnTakeDamage?.Invoke(this, EventArgs.Empty);

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
    public int GetHealth()
    {
        return _health;
    }
    public int GetMaxHealth()
    {
        return _maxHealth;
    }
    public float GetHealthPercentRemaining()
    {
        return (float)_health / _maxHealth;
    }
}
