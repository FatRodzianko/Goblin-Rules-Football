using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType
{
    Normal,
    Medic,
    Abnormal,
}
public enum UnitState
{
    None,
    Idle,
    Moving,
    Attacking,
    Defending,
    Melee,
    Shooting,
    Throwing
}
public class BombRunUnit : MonoBehaviour
{
    private const int ACTION_POINTS_MAX = 2;

    // static events
    public static event EventHandler OnAnyActionPointsChanged;
    public static event EventHandler OnAnyUnitSpawned;
    public static event EventHandler OnAnyUnitDied;
    public static event EventHandler OnAnyUnitMovedGridPosition;

    // non-static events
    public event EventHandler<BaseAction> OnActionTaken;
    public event EventHandler<DamageMode> OnDamageModeChanged;
    public event EventHandler<Vector2> OnActionDirectionChanged;
    public event EventHandler OnThisUnitMovedGridPosition;
    public event EventHandler<bool> OnUnitVisibilityChanged;
    public event EventHandler<UnitState> OnUnitStateChanged;


    [Header("Unit Info")]
    //[SerializeField] private int _startingHealth = 100;
    //[SerializeField] private int _health;
    [SerializeField] private UnitType _unitType;
    [SerializeField] private bool _isEnemy;
    [SerializeField] private DamageMode _damageMode;
    [SerializeField] private int _sightRange = 10;

    [Header("Unit State")]
    [SerializeField] private UnitState _unitState = UnitState.Idle;

    [Header("Unit Health Stuff")]
    [SerializeField] private BombRunUnitHealthSystem _healthSystem;

    [Header("GridPosition stuff")]
    private GridPosition _gridPosition;

    [Header("Actions")]
    [SerializeField] private BaseAction[] _baseActionArray;
    [SerializeField] private int _actionPoints = ACTION_POINTS_MAX;
    [SerializeField] private List<BaseAction> _actionsTakenThisTurn = new List<BaseAction>();

    [Header("Action Information")]
    [SerializeField] private Vector2 _actionDirection = Vector2.zero;

    [Header("Animation Stuff")]
    [SerializeField] private BombRunUnitAnimator _bombRunUnitAnimator;

    [Header("FOV Stuff?")]
    [SerializeField] private BombRunUnitFieldOfView _bombRunUnitFieldOfView;

    [Header("Fog of War Place Holder Object Stuff")]
    [SerializeField] Transform _invisibleUnitPlaceHolderObjectPrefab;
    [SerializeField] BombRunUnit_InvisibleUnitPlaceHolder _invisibleUnitPlaceHolder;

    [Header("Unit Visibility Stuff")]
    [SerializeField] private bool _hasUnitBeenSeen = false;
    [SerializeField] private Vector2Int _lastSeenAtVector2Position = new Vector2Int(-1, -1); 
    private GridPosition _lastSeenAtGridPosition;


    private void Awake()
    {
        _baseActionArray = GetComponents<BaseAction>();
        if (_healthSystem == null)
            _healthSystem = GetComponent<BombRunUnitHealthSystem>();
        if (_bombRunUnitAnimator == null)
        {
            _bombRunUnitAnimator = this.GetComponent<BombRunUnitAnimator>();
        }
        if (_bombRunUnitAnimator.GetSpriteFlipX())
        {
            SetActionDirection(new Vector2(-1, 0));
        }
        else
        {
            SetActionDirection(new Vector2(1, 0));
        }

        if (_bombRunUnitFieldOfView == null)
        {
            _bombRunUnitFieldOfView = this.transform.GetComponentInChildren<BombRunUnitFieldOfView>();
        }
    }
    private void Start()
    {
        //_gridPosition = LevelGrid.Instance.GetGridPositon(this.transform.position);
        //Debug.Log("BombRunUnit: Start: " + this.name + ": Starting position at: " + _gridPosition.ToString());
        //LevelGrid.Instance.AddUnitAtGridPosition(_gridPosition, this);

        //TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
        //_healthSystem.OnDead += HealthSystem_OnDead;

        //OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty);        

        UnitVisibilityManager_BombRun.OnAnyUnitBecameVisibile += UnitVisibilityManager_BombRun_OnAnyUnitBecameVisibile;
        UnitVisibilityManager_BombRun.OnAnyUnitBecameInVisibile += UnitVisibilityManager_BombRun_OnAnyUnitBecameInVisibile;
    }

    private void OnDisable()
    {
        TurnSystem.Instance.OnTurnChanged -= TurnSystem_OnTurnChanged;
        _healthSystem.OnDead -= HealthSystem_OnDead;

        UnitVisibilityManager_BombRun.OnAnyUnitBecameVisibile += UnitVisibilityManager_BombRun_OnAnyUnitBecameVisibile;
        UnitVisibilityManager_BombRun.OnAnyUnitBecameInVisibile += UnitVisibilityManager_BombRun_OnAnyUnitBecameInVisibile;
    }
    public void InitializeBombRunUnit()
    {
        _gridPosition = LevelGrid.Instance.GetGridPositon(this.transform.position);
        Debug.Log("BombRunUnit: InitializeBombRunUnit: " + this.name + ": Starting position at: " + _gridPosition.ToString());
        LevelGrid.Instance.AddUnitAtGridPosition(_gridPosition, this);

        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
        _healthSystem.OnDead += HealthSystem_OnDead;

        // hide unit to start?
        this.SetUnitVisibility(!this._isEnemy);


        _bombRunUnitFieldOfView.InitializeFOV();

        OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty);
    }
    private void Update()
    {
        GridPosition newGridPosition = LevelGrid.Instance.GetGridPositon(this.transform.position);
        if (newGridPosition != _gridPosition)
        {
            // unit changed grid position
            GridPosition oldGridPosition = _gridPosition;
            _gridPosition = newGridPosition;
            LevelGrid.Instance.UnitMovedGridPosition(this, oldGridPosition, newGridPosition);
            OnThisUnitMovedGridPosition?.Invoke(this, EventArgs.Empty);
            OnAnyUnitMovedGridPosition?.Invoke(this, EventArgs.Empty);
        }
    }
    public T GetAction<T>() where T : BaseAction
    {
        foreach (BaseAction baseAction in _baseActionArray)
        {
            if (baseAction is T)
            {
                return (T)baseAction;
            }
        }
        return null;
    }
    public BaseAction GetActionByActionType(ActionType actionType)
    {
        for (int i = 0; i < _baseActionArray.Length; i++)
        {
            if (_baseActionArray[i].GetActionType() == actionType)
            {
                return _baseActionArray[i];
            }
        }
        return null;
    }
    public GridPosition GetGridPosition()
    {
        return _gridPosition;
    }
    public Vector3 GetWorldPosition()
    {
        return this.transform.position;
    }
    public BaseAction[] GetBaseActionArray()
    {
        return _baseActionArray;
    }
    public bool TrySpendActionPointsToTakeAction(BaseAction baseAction, GridPosition actionPostion)
    {
        if (CanSpendActionPointsToTakeAction(baseAction, actionPostion))
        {
            //SpendActionPoints(baseAction.GetActionPointsCost());
            //AddActionTakenThisTurn(baseAction);
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool CanSpendActionPointsToTakeAction(BaseAction baseAction)
    {
        // re-write to call the CanTakeAction function from the base action? That way all actions can have custom logic for whether a player can take the action or not?
        return baseAction.CanTakeAction(_actionPoints, this.GetGridPosition());

    }
    public bool CanSpendActionPointsToTakeAction(BaseAction baseAction, GridPosition actionPosition)
    {
        // re-write to call the CanTakeAction function from the base action? That way all actions can have custom logic for whether a player can take the action or not?
        return baseAction.CanTakeAction(_actionPoints, actionPosition);

    }
    public void SpendActionPoints(int cost)
    {
        _actionPoints -= cost;
        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }
    public int GetActionPoints()
    {
        return _actionPoints;
    }
    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if ((this.IsEnemy() && !TurnSystem.Instance.IsPlayerTurn()) || // if the unit IS an enemy and it IS NOT the player's turn aka, the unit is an enemy and it is the enemy turn
            (!this.IsEnemy() && TurnSystem.Instance.IsPlayerTurn())) // if the unit IS NOT an enemy and it IS the player's turn, aka the unit is the player's and it is the player's turn
        {
            _actionPoints = ACTION_POINTS_MAX;
            ClearActionsTakenThisTurn();
            OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
        }
        
    }
    private void ClearActionsTakenThisTurn()
    {
        _actionsTakenThisTurn.Clear();
    }
    public void AddActionTakenThisTurn(BaseAction action)
    {
        if (action == null)
            return;

        if (!_actionsTakenThisTurn.Contains(action))
        {
            _actionsTakenThisTurn.Add(action);
        }

        OnActionTaken?.Invoke(this, action);
    }
    public void SetIsEnemy(bool isEnemy)
    {
        this._isEnemy = isEnemy;
    }
    public bool IsEnemy()
    {
        return _isEnemy;
    }
    public void DamageAllBodyParts()
    {
        foreach (BombRunUnitBodyPartAndFrozenState bodyPartAndFrozenState in this.GetUnitHealthSystem().GetAllBodyPartsAndFrozenState())
        {
            this.DamageBodyPart(bodyPartAndFrozenState.BodyPart);
        }
    }
    public void DamageBodyPart(BodyPart bodyPart)
    {
        _healthSystem.TakeDamageToBodyPart(bodyPart);
    }
    public void HealBodyPart(BodyPart bodyPart)
    {
        _healthSystem.HealDamageOnBodyPart(bodyPart);
    }
    public void HealAllBodyParts()
    {
        foreach (BombRunUnitBodyPartAndFrozenState bodyPartAndFrozenState in this.GetUnitHealthSystem().GetAllBodyPartsAndFrozenState())
        {
            this.HealBodyPart(bodyPartAndFrozenState.BodyPart);
        }
    }
    private void HealthSystem_OnDead(object sender, EventArgs e)
    {
        KillUnit();
    }
    private void KillUnit()
    {
        LevelGrid.Instance.RemoveUnitAtGridPosition(this._gridPosition, this);

        Destroy(this.gameObject);

        OnAnyUnitDied?.Invoke(this, EventArgs.Empty);
    }
    public BombRunUnitAnimator GetUnitAnimator()
    {
        if (_bombRunUnitAnimator == null)
        {
            FindAnimatorScript();
        }
        return _bombRunUnitAnimator;
    }
    private void FindAnimatorScript()
    {
        if (TryGetComponent<BombRunUnitAnimator>(out BombRunUnitAnimator animator))
        {
            _bombRunUnitAnimator = animator;
        }

        // if not found on parent, try children
        if (_bombRunUnitAnimator == null)
        {
            Transform[] allChildren = GetComponentsInChildren<Transform>();
            for (int i = 0; i < allChildren.Length; i++)
            {
                if (allChildren[i].TryGetComponent<BombRunUnitAnimator>(out BombRunUnitAnimator childAnimator))
                {
                    _bombRunUnitAnimator = childAnimator;
                    break;
                }
            }
        }
    }
    public BombRunUnitHealthSystem GetUnitHealthSystem()
    {
        return _healthSystem;
    }
    public int GetRemainingHealth()
    {
        return _healthSystem.GetHealth();
    }
    public void SetUnitType(UnitType unitType)
    {
        _unitType = unitType;
    }
    public UnitType GetUnitType()
    {
        return _unitType;
    }
    public DamageMode GetDamageMode()
    {
        return _damageMode;
    }
    public void SetDamageMode(DamageMode damageMode)
    {
        if (_unitType == UnitType.Medic)
        {
            _damageMode = DamageMode.Medic;
            return;
        }

        _damageMode = damageMode;
    }
    public void SetUnitSightRange(int sightRange)
    {
        this._sightRange = sightRange;
    }
    public int GetSightRange()
    {
        return _sightRange;
    }
    public Vector2 GetActionDirection()
    {
        return _actionDirection;
    }
    public void SetActionDirection(Vector2 direction)
    {
        if (this._healthSystem.GetBodyPartFrozenState(BodyPart.Head) == BodyPartFrozenState.FullFrozen)
        {
            Debug.Log("SetActionDirection: Cannot change aim direction because head is frozen");
            return;
        }
        _actionDirection = direction.normalized;
        OnActionDirectionChanged?.Invoke(this, _actionDirection);
    }
    public bool CanUnitSeeThisPosition(GridPosition gridPosition)
    {
        bool canUnitSeeThisPosition = _bombRunUnitFieldOfView.HasThisUnitSeenThisGridPosition(gridPosition);
        //Debug.Log("CanUnitSeeThisPosition: " + this.name + " can see: " + gridPosition.ToString() +"?: " + canUnitSeeThisPosition);
        return canUnitSeeThisPosition;
    }
    public bool CanUnitSeeThisUnit(BombRunUnit unit)
    {
        bool canUnitSeeThisUnit = _bombRunUnitFieldOfView.CanUnitSeeThisUnit(unit);
        //Debug.Log("CanUnitSeeThisUnit: " + this.name + " can see: " +unit.name.ToString() + "?: " + canUnitSeeThisUnit);
        return canUnitSeeThisUnit;
    }
    public void SetUnitVisibility(bool isVisible)
    {
        //Debug.Log("SetUnitVisibility: " + this.name + " isVisible: " + isVisible);
        //this._animator.SetUnitVisibility(isVisible);
        OnUnitVisibilityChanged?.Invoke(this, isVisible);
    }
    public bool GetUnitVisibility()
    {
        return this._bombRunUnitAnimator.GetUnitVisibility();
    }
    public List<GridPosition> GetUnitsVisibileGridPositions()
    {
        return _bombRunUnitFieldOfView.GetUnitsVisibileGridPositions();
    }
    public BombRunUnitFieldOfView GetBombRunUnitFieldOfView()
    {
        return _bombRunUnitFieldOfView;
    }
    public Sprite GetCurrentSprite()
    {
        return _bombRunUnitAnimator.GetCurrentSprite();
    }
    public SpriteRenderer GetSpriteRenderer()
    {
        return _bombRunUnitAnimator.GetSpriteRenderer();
    }
    public UnitAnimationState GetUnitAnimationState()
    {
        return _bombRunUnitAnimator.GetUnitAnimationState();
    }
    public bool GetSpriteFlipX()
    {
        return _bombRunUnitAnimator.GetSpriteFlipX();
    }
    private void UnitVisibilityManager_BombRun_OnAnyUnitBecameVisibile(object sender, BombRunUnit unit)
    {
        // for multiplayer, check if this unit is owned by the local player. For now, just have it check if it is an enemy or not?
        if (!this._isEnemy)
        {
            return;
        }
        if (this == unit)
        {
            //Debug.Log("UnitVisibilityManager_BombRun_OnAnyUnitBecameVisibile: " + this.name + " became visibile");
            DestroyInvisibleUnitPlaceHolder();
        }
    }
    private void UnitVisibilityManager_BombRun_OnAnyUnitBecameInVisibile(object sender, BombRunUnit unit)
    {
        // for multiplayer, check if this unit is owned by the local player. For now, just have it check if it is an enemy or not?
        if (!this._isEnemy)
        {
            return;
        }

        if (this == unit)
        {
            //Debug.Log("UnitVisibilityManager_BombRun_OnAnyUnitBecameInVisibile: " + unit.name + " become INVISIBLE");
            DestroyInvisibleUnitPlaceHolder();
            SpawnInvisibleUnitPlaceHolder();
        }
    }
    private void InvisibleUnitPlaceHolder_OnGridPositionBecameVisible(object sender, EventArgs e)
    {
        DestroyInvisibleUnitPlaceHolder();
    }
    private void SpawnInvisibleUnitPlaceHolder()
    {
        // Get Invisible Unit spawn position
        Vector3 spawnPosition = LevelGrid.Instance.GetWorldPosition(this.GetGridPosition());
        if (this.GetUnitAnimationState() == UnitAnimationState.Moving)
        {
            spawnPosition = this.GetWorldPosition();
        }

        // spawn the invisilbe unit placeholder
        Transform invibleUnitPlaceHolderObject = Instantiate(_invisibleUnitPlaceHolderObjectPrefab, spawnPosition, Quaternion.identity);
        _invisibleUnitPlaceHolder = invibleUnitPlaceHolderObject.GetComponent<BombRunUnit_InvisibleUnitPlaceHolder>();
        _invisibleUnitPlaceHolder.InitializeInvisibleUnitPlaceHolder(this);

        // event to destroy placeholder object if its grid position is seen
        _invisibleUnitPlaceHolder.OnGridPositionBecameVisible += InvisibleUnitPlaceHolder_OnGridPositionBecameVisible;
    }    

    private void DestroyInvisibleUnitPlaceHolder()
    {
        try
        {
            if (this._invisibleUnitPlaceHolder != null)
            {
                _invisibleUnitPlaceHolder.OnGridPositionBecameVisible -= InvisibleUnitPlaceHolder_OnGridPositionBecameVisible;
                GameObject.Destroy(_invisibleUnitPlaceHolder.gameObject);
            }
            this._invisibleUnitPlaceHolder = null;
        }
        catch (Exception e)
        {
            Debug.Log("DestroyInvisibleUnitPlaceHolder: " + e);
        }
        
    }
    public UnitState GetUnitState()
    {
        return _unitState;
    }
    public void SetUnitState(UnitState unitState)
    {
        this._unitState = unitState;
        OnUnitStateChanged?.Invoke(this, this._unitState);
    }
    public bool HasUnitBeenSeenYet()
    {
        return _hasUnitBeenSeen;
    }
    public void SetHasUnitBeenSeen(bool hasUnitBeenSeen)
    {
        this._hasUnitBeenSeen = hasUnitBeenSeen;
    }
    public GridPosition LastSeenAtGridPosition()
    {
        return _lastSeenAtGridPosition;
    }
    public void UnitSeenByEnemyUnit()
    {
        if (!_hasUnitBeenSeen)
            _hasUnitBeenSeen = true;

        this._lastSeenAtGridPosition = this._gridPosition;
        this._lastSeenAtVector2Position = new Vector2Int(_gridPosition.x, _gridPosition.y);
    }
    public void SetUnitAnimatorController(RuntimeAnimatorController animatorController)
    {
        this._bombRunUnitAnimator.SetUnitAnimatorController(animatorController);
    }
}
