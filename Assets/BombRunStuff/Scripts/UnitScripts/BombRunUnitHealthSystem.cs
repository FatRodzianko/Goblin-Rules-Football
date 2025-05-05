using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BodyPartFrozenState
{
    None,
    HalfFrozen,
    FullFrozen
}
public enum BodyPart
{
    Head,
    Arms,
    Legs
}
[Serializable]
public class BombRunUnitBodyPart
{
    public BodyPart BodyPart;
    public BodyPartFrozenState BodyPartFrozenState;
}
public class BombRunUnitHealthSystem : MonoBehaviour
{

    // events
    public event EventHandler OnDead;
    public event EventHandler OnTakeDamage;

    [SerializeField] private int _startingHealth = 100;
    [SerializeField] private int _health = 100;
    [SerializeField] private int _maxHealth;

    [Header("Body Parts: Frozen State")]
    [SerializeField] private List<BombRunUnitBodyPart> _bodyParts = new List<BombRunUnitBodyPart>();
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
    public List<BombRunUnitBodyPart> GetAllBodyParts()
    {
        return _bodyParts;
    }
    public BombRunUnitBodyPart GetSpecificBodyPart(BodyPart bodyPart)
    {
        if (_bodyParts.Exists(x => x.BodyPart == bodyPart))
        {
            return _bodyParts.First(x => x.BodyPart == bodyPart);
        }
        else
        {
            return null;
        }
    }
    public BodyPartFrozenState GetBodyPartFrozenState(BodyPart bodyPart)
    {
        if (_bodyParts.Exists(x => x.BodyPart == bodyPart))
        {
            BombRunUnitBodyPart newBodyPart = _bodyParts.First(x => x.BodyPart == bodyPart);
            return newBodyPart.BodyPartFrozenState;
        }
        else
        {
            return BodyPartFrozenState.None;
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
        if (GetSpecificBodyPart(BodyPart.Legs).BodyPartFrozenState != BodyPartFrozenState.None)
        {
            UnFreezeBodyPart(BodyPart.Legs);
        }
        else if (GetSpecificBodyPart(BodyPart.Arms).BodyPartFrozenState != BodyPartFrozenState.None)
        {
            UnFreezeBodyPart(BodyPart.Arms);
        }
        else if (GetSpecificBodyPart(BodyPart.Head).BodyPartFrozenState != BodyPartFrozenState.None)
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
        if (!_bodyParts.Exists(x => x.BodyPart == bodyPart))
        {
            Debug.Log("FreezeBodyPart: could not find body part: " + bodyPart.ToString());
        }
        
        BombRunUnitBodyPart bombRunUnitBodyPart = _bodyParts.First(x => x.BodyPart == bodyPart);

        switch (bombRunUnitBodyPart.BodyPartFrozenState)
        {
            case BodyPartFrozenState.None:
                bombRunUnitBodyPart.BodyPartFrozenState = BodyPartFrozenState.HalfFrozen;
                break;
            case BodyPartFrozenState.HalfFrozen:
                bombRunUnitBodyPart.BodyPartFrozenState = BodyPartFrozenState.FullFrozen;
                break;
            case BodyPartFrozenState.FullFrozen:
                break;
        }
    }
    public void UnFreezeBodyPart(BodyPart bodyPart)
    {
        Debug.Log("UnFreezeBodyPart: " + bodyPart.ToString());
        if (!_bodyParts.Exists(x => x.BodyPart == bodyPart))
        {
            Debug.Log("UnFreezeBodyPart: could not find body part: " + bodyPart.ToString());
        }

        BombRunUnitBodyPart bombRunUnitBodyPart = _bodyParts.First(x => x.BodyPart == bodyPart);

        switch (bombRunUnitBodyPart.BodyPartFrozenState)
        {
            case BodyPartFrozenState.None:
                break;
            case BodyPartFrozenState.HalfFrozen:
                bombRunUnitBodyPart.BodyPartFrozenState = BodyPartFrozenState.None;
                break;
            case BodyPartFrozenState.FullFrozen:
                bombRunUnitBodyPart.BodyPartFrozenState = BodyPartFrozenState.HalfFrozen;
                break;
        }
    }
}
