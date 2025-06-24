using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BodyPartFrozenState
{
    NotFrozen,
    HalfFrozen,
    FullFrozen
}
public enum BodyPart
{
    None,
    Head,
    Arms,
    Legs
}
[Serializable]
public class BombRunUnitBodyPartAndFrozenState
{
    public BodyPart BodyPart;
    public BodyPartFrozenState BodyPartFrozenState;
}
public class BombRunUnitHealthSystem : MonoBehaviour
{
    // static events
    public static event EventHandler OnAnyBodyPartFrozenStateChanged;
    // events
    public event EventHandler OnDead;
    public event EventHandler OnTakeDamage;
    public event EventHandler<BodyPart> OnBodyPartFrozenStateChanged;

    [Header("Unit Details?")]
    [SerializeField] private BombRunUnit _unit;
    [SerializeField] private int _startingHealth = 100;
    [SerializeField] private int _health = 100;
    [SerializeField] private int _maxHealth;

    [Header("Body Parts: Frozen State")]
    [SerializeField] private List<BombRunUnitBodyPartAndFrozenState> _bodyPartsAndFrozenState = new List<BombRunUnitBodyPartAndFrozenState>();
    private bool _fullyFrozen = false;

    private void Awake()
    {
        _health = _startingHealth;
        _maxHealth = _startingHealth;
    }
    public void TakeDamage(int damageAmount)
    {
        //_health -= damageAmount;

        if (_fullyFrozen)
            TestUnFreezeBodyPart();
        else
            TestFreezeBodyPart();

        OnTakeDamage?.Invoke(this, EventArgs.Empty);

        if (_health <= 0)
        {
            _health = 0;
            Die();
        }
    }
    public void TakeDamageToBodyPart(BodyPart bodyPart)
    {
        FreezeBodyPart(bodyPart);
    }
    public void HealDamageOnBodyPart(BodyPart bodyPart)
    {
        UnFreezeBodyPart(bodyPart);
    }
    private void Die()
    {
        OnDead?.Invoke(this, EventArgs.Empty);
    }
    public BombRunUnit GetUnit()
    {
        return _unit;
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
    public List<BombRunUnitBodyPartAndFrozenState> GetAllBodyPartsAndFrozenState()
    {
        return _bodyPartsAndFrozenState;
    }
    public BombRunUnitBodyPartAndFrozenState GetSpecificBodyPart(BodyPart bodyPart)
    {
        if (_bodyPartsAndFrozenState.Exists(x => x.BodyPart == bodyPart))
        {
            return _bodyPartsAndFrozenState.First(x => x.BodyPart == bodyPart);
        }
        else
        {
            return null;
        }
    }
    public BodyPartFrozenState GetBodyPartFrozenState(BodyPart bodyPart)
    {
        if (_bodyPartsAndFrozenState.Exists(x => x.BodyPart == bodyPart))
        {
            BombRunUnitBodyPartAndFrozenState newBodyPart = _bodyPartsAndFrozenState.First(x => x.BodyPart == bodyPart);
            return newBodyPart.BodyPartFrozenState;
        }
        else
        {
            return BodyPartFrozenState.NotFrozen;
        }
    }
    public void TestFreezeBodyPart()
    {
        if (GetSpecificBodyPart(BodyPart.Legs).BodyPartFrozenState != BodyPartFrozenState.FullFrozen)
        {
            FreezeBodyPart(BodyPart.Legs);
        }
        else if (GetSpecificBodyPart(BodyPart.Arms).BodyPartFrozenState != BodyPartFrozenState.FullFrozen)
        {
            FreezeBodyPart(BodyPart.Arms);
        }
        else if (GetSpecificBodyPart(BodyPart.Head).BodyPartFrozenState != BodyPartFrozenState.FullFrozen)
        {
            FreezeBodyPart(BodyPart.Head);
        }
        else
        {
            _fullyFrozen = true;
            TestUnFreezeBodyPart();
        }
    }
    public void TestUnFreezeBodyPart()
    {
        if (GetSpecificBodyPart(BodyPart.Legs).BodyPartFrozenState != BodyPartFrozenState.NotFrozen)
        {
            UnFreezeBodyPart(BodyPart.Legs);
        }
        else if (GetSpecificBodyPart(BodyPart.Arms).BodyPartFrozenState != BodyPartFrozenState.NotFrozen)
        {
            UnFreezeBodyPart(BodyPart.Arms);
        }
        else if (GetSpecificBodyPart(BodyPart.Head).BodyPartFrozenState != BodyPartFrozenState.NotFrozen)
        {
            UnFreezeBodyPart(BodyPart.Head);
        }
        else
        {
            _fullyFrozen = false;
            TestFreezeBodyPart();
        }
    }
    public void FreezeBodyPart(BodyPart bodyPart)
    {
        Debug.Log("FreezeBodyPart: " + bodyPart.ToString());
        if (!_bodyPartsAndFrozenState.Exists(x => x.BodyPart == bodyPart))
        {
            Debug.Log("FreezeBodyPart: could not find body part: " + bodyPart.ToString());
            return;
        }
        
        BombRunUnitBodyPartAndFrozenState bombRunUnitBodyPart = _bodyPartsAndFrozenState.First(x => x.BodyPart == bodyPart);

        switch (bombRunUnitBodyPart.BodyPartFrozenState)
        {
            case BodyPartFrozenState.NotFrozen:
                bombRunUnitBodyPart.BodyPartFrozenState = BodyPartFrozenState.HalfFrozen;
                break;
            case BodyPartFrozenState.HalfFrozen:
                bombRunUnitBodyPart.BodyPartFrozenState = BodyPartFrozenState.FullFrozen;
                break;
            case BodyPartFrozenState.FullFrozen:
                break;
        }

        OnBodyPartFrozenStateChanged?.Invoke(this, bombRunUnitBodyPart.BodyPart);
        OnAnyBodyPartFrozenStateChanged?.Invoke(this, EventArgs.Empty);
    }
    public void UnFreezeBodyPart(BodyPart bodyPart)
    {
        Debug.Log("UnFreezeBodyPart: " + bodyPart.ToString());
        if (!_bodyPartsAndFrozenState.Exists(x => x.BodyPart == bodyPart))
        {
            Debug.Log("UnFreezeBodyPart: could not find body part: " + bodyPart.ToString());
        }

        BombRunUnitBodyPartAndFrozenState bombRunUnitBodyPart = _bodyPartsAndFrozenState.First(x => x.BodyPart == bodyPart);

        switch (bombRunUnitBodyPart.BodyPartFrozenState)
        {
            case BodyPartFrozenState.NotFrozen:
                break;
            case BodyPartFrozenState.HalfFrozen:
                bombRunUnitBodyPart.BodyPartFrozenState = BodyPartFrozenState.NotFrozen;
                break;
            case BodyPartFrozenState.FullFrozen:
                bombRunUnitBodyPart.BodyPartFrozenState = BodyPartFrozenState.HalfFrozen;
                break;
        }
        OnBodyPartFrozenStateChanged?.Invoke(this, bombRunUnitBodyPart.BodyPart);
        OnAnyBodyPartFrozenStateChanged?.Invoke(this, EventArgs.Empty);
    }
    public bool AreAllBodyPartsFrozen()
    {
        foreach (BombRunUnitBodyPartAndFrozenState bodyPart in this._bodyPartsAndFrozenState)
        {
            if (bodyPart.BodyPartFrozenState != BodyPartFrozenState.FullFrozen)
            {
                return false;
            }
        }
        return true;
    }
    public bool AreAnyBodyPartsFrozen()
    {
        foreach (BombRunUnitBodyPartAndFrozenState bodyPart in this._bodyPartsAndFrozenState)
        {
            if (bodyPart.BodyPartFrozenState != BodyPartFrozenState.NotFrozen)
            {
                return true;
            }
        }
        return false;
    }
}

