using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnitFieldOfView : MonoBehaviour
{
    [SerializeField] private BombRunUnit _unit;

    [Header("FOV Parameters")]
    [SerializeField] private int _rayCount = 50;
    [SerializeField] private float _fov = 90f;
    [SerializeField] private MeshFilter _meshFilter;
    private int _frameCounter = 0;

    
    [Header("Raycast stuff?")]
    [SerializeField] private LayerMask _blockingLayer;
    [SerializeField] private List<LayerMask> _blockingLayers = new List<LayerMask>();
    [SerializeField] private LayerMask _obstacleLayer;
    [SerializeField] private LayerMask _unitLayer;
    private Vector3 _origin = Vector3.zero;
    private float _startingAngle = 0f;
    private float _aimDirectionAngle = 0f;
    [SerializeField] private Vector2 _aimDirectionVector = Vector2.zero;
    

    [Header("Unit movement and other triggers for mesh creation")]
    [SerializeField] private bool _isMoving = false;

    [Header("FOV Materials and appearance")]
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private Material _friendlyFOVMaterial;
    [SerializeField] private Material _enemyFOVMaterial;
    private Mesh _mesh;

    [Header("Colliders and stuff")]
    [SerializeField] private PolygonCollider2D _collider;

    [Header("Visibile Units")]
    [SerializeField] private List<BombRunUnit> _visibleUnits = new List<BombRunUnit>();
    private List<GridPosition> _visibleGridPositions = new List<GridPosition>();
    [SerializeField] private List<Vector2> _visibleVector2Positions = new List<Vector2>();

    private void Awake()
    {
        if (_meshFilter == null)
            this._meshFilter = this.GetComponent<MeshFilter>();
        if (_unit == null)
            this._unit = this.transform.parent.GetComponent<BombRunUnit>();
    }
    private void Start()
    {
        //_mesh = new Mesh();
        //_meshFilter.mesh = _mesh;

        // subscribe to events?
        _unit.OnActionDirectionChanged += Unit_OnActionDirectionChanged;
        _unit.OnThisUnitMovedGridPosition += Unit_OnThisUnitMovedGridPosition;
        if (_unit.TryGetComponent<MoveAction>(out MoveAction moveAction))
        {
            moveAction.OnStartMoving += MoveAction_OnStartMoving;
            moveAction.OnStopMoving += MoveAction_OnStopMoving;
        }

        LevelGrid.Instance.OnWallsAndFloorsPlacedCompleted += LevelGrid_OnWallsAndFloorsPlacedCompleted;
        BaseBombRunObstacle.OnAnyObstacleCoverTypeChanged += BombRunObstacle_OnAnyObstacleCoverTypeChanged;
    }

    

    private void OnDisable()
    {
        _unit.OnActionDirectionChanged -= Unit_OnActionDirectionChanged;
        _unit.OnThisUnitMovedGridPosition -= Unit_OnThisUnitMovedGridPosition;
        if (_unit.TryGetComponent<MoveAction>(out MoveAction moveAction))
        {
            moveAction.OnStartMoving -= MoveAction_OnStartMoving;
            moveAction.OnStopMoving -= MoveAction_OnStopMoving;
        }
        LevelGrid.Instance.OnWallsAndFloorsPlacedCompleted -= LevelGrid_OnWallsAndFloorsPlacedCompleted;
        BaseBombRunObstacle.OnAnyObstacleCoverTypeChanged -= BombRunObstacle_OnAnyObstacleCoverTypeChanged;
    }
    public void InitializeFOV()
    {
        _mesh = new Mesh();
        _meshFilter.mesh = _mesh;

        SetFOVMaterial(_unit.IsEnemy());
        SetAimDirection(_unit.GetActionDirection());
        //UpdateFOVMesh();
        if(isActiveAndEnabled)
            StartCoroutine(DelayForWallCollidersToSpawn(0.25f));
    }
    
    IEnumerator DelayForWallCollidersToSpawn(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        //Debug.Log("DelayForWallCollidersToSpawn: waited for " + waitTime);
        //UpdateFOVMesh();
        GetVisibileGridPositions();
    }
    private void SetFOVMaterial(bool isEnemy)
    {
        if (isEnemy)
        {
            _meshRenderer.material = _enemyFOVMaterial;
        }
        else
        {
            _meshRenderer.material = _friendlyFOVMaterial;
        }

    }
    private void MoveAction_OnStartMoving(object sender, EventArgs e)
    {
        _isMoving = true;
    }
    private void MoveAction_OnStopMoving(object sender, EventArgs e)
    {
        _isMoving = false;
    }
    private void LevelGrid_OnWallsAndFloorsPlacedCompleted(object sender, EventArgs e)
    {
        Debug.Log("LevelGrid_OnWallsAndFloorsPlacedCompleted: " + _unit.name);
        //UpdateFOVMesh();
        GetVisibileGridPositions();
    }
    private void LateUpdate()
    {

        if (!_isMoving)
            return;
        //UpdateFOVMesh();
        //if (_frameCounter == 0)
        //{
        //    UpdateFOVMesh();
        //}
        //if (_frameCounter >= 2)
        //{
        //    _frameCounter = 0;
        //    return;
        //}
        //_frameCounter++;
        
    }
    private void UpdateFOVMeshWithGridPositions(List<GridPosition> gridPositions)
    {
        // setup field of view parameters
        _origin = _unit.transform.position;

        // initialize the arrays
        Vector3[] vertices = new Vector3[gridPositions.Count + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[gridPositions.Count * 3];

        // initialize polygon collider points array
        Vector2[] polygonColliderPoints = new Vector2[gridPositions.Count + 1 + 1];

        // set the origin
        vertices[0] = _origin;
        polygonColliderPoints[0] = _origin;

        // cycle through rays and add new vertex
        int vertexIndex = 1;
        int triangleIndex = 0;

        // cycle through the grid positions, add vertex for the grid position?
        if (gridPositions.Count > 0)
        {
            for (int i = 0; i <= gridPositions.Count; i++)
            {
                Vector3 vertex = LevelGrid.Instance.GetWorldPosition(gridPositions[i]);


                vertices[vertexIndex] = vertex;
                polygonColliderPoints[vertexIndex] = vertex;

                if (i > 0)
                {
                    // set the points of the triangle
                    triangles[triangleIndex + 0] = 0;
                    triangles[triangleIndex + 1] = vertexIndex - 1;
                    triangles[triangleIndex + 2] = vertexIndex;
                    // increase triangle index by 3, since we are adding 3 points every loop
                    triangleIndex += 3;
                }

                vertexIndex++;
            }
        }
        

        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;
        _mesh.bounds = new Bounds(_origin, Vector3.one * 500f);

        this.transform.position = Vector3.zero;

        _collider.SetPath(0, polygonColliderPoints);
    }
    private void UpdateFOVMesh()
    {
        // setup field of view parameters
        _origin = _unit.transform.position;

        // for testing: aim direction will be where mouse is relative to unit
        float angle = _startingAngle;
        float angleIncrease = _fov / _rayCount;
        float viewDistance = _unit.GetSightRange() * LevelGrid.Instance.GetGridCellSize();

        // initialize the arrays
        Vector3[] vertices = new Vector3[_rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[_rayCount * 3];

        // initialize polygon collider points array
        Vector2[] polygonColliderPoints = new Vector2[_rayCount + 1 + 1];

        // set the origin
        vertices[0] = _origin;
        polygonColliderPoints[0] = _origin;

        // cycle through rays and add new vertex
        int vertexIndex = 1;
        int triangleIndex = 0;

        // track spotted goblins
        List<BombRunUnit> spottedEnemyUnits = new List<BombRunUnit>();
        for (int i = 0; i <= _rayCount; i++)
        {
            //Debug.Log("FieldOfView: Vertex index: " + vertexIndex.ToString() + " with max verticies: " + vertices.Length.ToString() + " for unit: " + _unit.name);
            Vector3 vectorFromAngle = GetVectorFromAngle(angle);
            Vector3 vertex = _origin + vectorFromAngle * viewDistance;

            float closestHitDistance = 0f;
            bool firstIteration = true;
            foreach (LayerMask layerMask in _blockingLayers)
            {
                // Change to a Physics2D.RaycastAll because if there is an obstacle you skip over, you'll want to see if there are any other obstacles behind that
                // for collision with walls, just end the for loop after the first one?
                RaycastHit2D[] raycastHits2D = Physics2D.RaycastAll(_origin, vectorFromAngle, viewDistance, layerMask);

                if (raycastHits2D.Length == 0)
                    continue;

                Vector3 hitPoint = vertex;
                for (int j = 0; j < raycastHits2D.Length; j++)
                {
                    if (raycastHits2D[j].collider.CompareTag("BombRunWall"))
                    {
                        hitPoint = raycastHits2D[j].point;
                        break;
                    }
                    if (raycastHits2D[j].transform.TryGetComponent<BaseBombRunObstacle>(out BaseBombRunObstacle obstacle))
                    {
                        if (obstacle.IsWalkable() || obstacle.GetObstacleCoverType() != ObstacleCoverType.Full)
                        {
                            continue;
                        }
                        hitPoint = raycastHits2D[j].point;
                        break;
                    }
                }
                float newDistance = Vector2.Distance(_origin, hitPoint);
                if (firstIteration)
                {
                    closestHitDistance = newDistance;
                    vertex = hitPoint;
                    firstIteration = false;
                    continue;
                }
                if (newDistance <= closestHitDistance)
                {
                    closestHitDistance = newDistance;
                    vertex = hitPoint;
                }

            }

            vertices[vertexIndex] = vertex;
            polygonColliderPoints[vertexIndex] = vertex;

            if (i > 0)
            {
                // set the points of the triangle
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;
                // increase triangle index by 3, since we are adding 3 points every loop
                triangleIndex += 3;
            }

            // See if any enemy units were seen?
            //spottedEnemyUnits.AddRange(FindVisibleEnemyUnits(spottedEnemyUnits, _origin, vectorFromAngle, Vector2.Distance(_origin, vertex) - 0.1f)); // the -0.1f is to make it so the ray won't go all the way to this hit object? In event unit and obstacle are overlapping at exact point? idk
            //if (_frameCounter > 10 || !_isMoving)
            //    spottedEnemyUnits.AddRange(FindVisibleEnemyUnits(spottedEnemyUnits, _origin, vectorFromAngle, Vector2.Distance(_origin, vertex) - 0.1f)); // the -0.1f is to make it so the ray won't go all the way to this hit object? In event unit and obstacle are overlapping at exact point? idk
            vertexIndex++;
            // increase angle for next loop. subtract to go clockwise
            angle -= angleIncrease;
        }

        // this ends up getting called way to often. Maybe have it so it's only called every so often?
        // Have a list of units spotted while moving. while isMoving is true, check to see if you spotted any new units. If so, add to new unit list, and update the UnitVisibilityManager with just that one unit?
        // when the unit stops moving, then submit to UnitCompletedFOVCheck? So it's only happening
        //UnitVisibilityManager_BombRun.Instance.UnitCompletedFOVCheck(this._unit, spottedEnemyUnits);
        //if (_frameCounter > 10 || !_isMoving)
        //{
        //    UnitVisibilityManager_BombRun.Instance.UnitCompletedFOVCheck(this._unit, spottedEnemyUnits);
        //    _frameCounter = 0;
        //}
        //_frameCounter++;

        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;
        _mesh.bounds = new Bounds(_origin, Vector3.one * 500f);

        this.transform.position = Vector3.zero;

        _collider.SetPath(0, polygonColliderPoints);
    }
    private List<BombRunUnit> FindVisibleEnemyUnits(List<BombRunUnit> alreadySpottedGoblins, Vector2 origin, Vector2 direction, float distance)
    {
        List<BombRunUnit> spottedGoblins = new List<BombRunUnit>();
        spottedGoblins.AddRange(alreadySpottedGoblins);

        RaycastHit2D[] raycastHits2D = Physics2D.RaycastAll(_origin, direction, distance, _unitLayer);
        if (raycastHits2D.Length == 0)
            return spottedGoblins;

        for (int i = 0; i < raycastHits2D.Length; i++)
        {
            BombRunUnit unit = raycastHits2D[i].collider.GetComponent<BombRunUnit>();
            if (unit.IsEnemy() == this._unit.IsEnemy())
                continue;
            if (!spottedGoblins.Contains(unit))
            {
                spottedGoblins.Add(unit);
            }
        }
        return spottedGoblins;
    }
    public void SetOrigin(Vector3 origin)
    {
        this._origin = origin;
    }
    private void Unit_OnActionDirectionChanged(object sender, Vector2 actionDirection)
    {
        SetAimDirection(actionDirection);
        if (!_isMoving)
        {
            //UpdateFOVMesh();
        }
        GetVisibileGridPositions();
    }
    public void SetAimDirection(Vector3 aimDirection)
    {
        // convert to angle set to 8 direction movement
        _aimDirectionAngle = GetAngleFromVectorFloat(aimDirection.normalized, true);
        _startingAngle = _aimDirectionAngle + _fov / 2f;
        _aimDirectionVector = GetVectorFromAngle(_aimDirectionAngle);
    }
    
    public Vector3 GetVectorFromAngle(float angle)
    {
        // angle = 0 -> 360
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }
    public float GetAngleFromVectorFloat(Vector3 dir, bool forceEightDirections = false)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;

        // https://www.reddit.com/r/Unity3D/comments/r2ykn2/new_unity_input_system_how_to_make_controller/
        if (forceEightDirections)
        {
            n = Mathf.Round(n / 45.0f) * 45.0f;
        }

        return n;
    }
    public bool IsPositionInFOVCollider(Vector3 position)
    {
        return _collider.OverlapPoint(position);
    }
    public bool CanUnitSeeThisUnit(BombRunUnit unit)
    {
        return HasThisUnitSeenThisGridPosition(unit.GetGridPosition());
        //if (_visibleUnits.Contains(unit))
        //    return true;

        //if (_visibleGridPositions.Contains(unit.GetGridPosition()))
        //    return true;

        //return false;

        //bool canUnitSeePosition = CanUnitSeeGridPosition(unit.GetGridPosition());
        //return canUnitSeePosition;
    }
    public bool HasThisUnitSeenThisGridPosition(GridPosition gridPosition)
    {
        return _visibleGridPositions.Contains(gridPosition);
    }
    private void BombRunObstacle_OnAnyObstacleCoverTypeChanged(object sender, GridPosition e)
    {
        if (HasThisUnitSeenThisGridPosition(e))
        {
            Debug.Log("BombRunObstacle_OnAnyObstacleCoverTypeChanged: updating position: " + e + " for: " + this._unit);
            GetVisibileGridPositions();
        }
    }
    public bool CanUnitSeeWorldPosition(Vector3 position)
    {
        //Debug.Log("CanUnitSeeWorldPosition: " + position.ToString());
        //Vector3 unitPosition = _unit.transform.position;
        Vector3 unitPosition = LevelGrid.Instance.GetWorldPosition(_unit.GetGridPosition());

        // check distance to position. If it is greater than the unit's view distance, return false
        float viewDistance = _unit.GetSightRange() * LevelGrid.Instance.GetGridCellSize();
        float distanceToPosition = Vector2.Distance(unitPosition, position);
        if (distanceToPosition > viewDistance)
        {
            //Debug.Log("CanUnitSeeWorldPosition: " + this._unit.name + ": Position: " + position.ToString() + " is too far to see");
            return false;
        }

        // check the angle from the player to the position. If it is outside of FOV, return false
        Vector3 directionToPosition = (position - unitPosition).normalized;
        //SetAimDirection(_unit.GetActionDirection());
        Vector3 vectorFromAngle = GetVectorFromAngle(_aimDirectionAngle);
        //Vector3 vectorFromAngle = GetVectorFromAngle(GetAngleFromVectorFloat(_unit.GetActionDirection(), true));
        //float angle = Vector3.Angle(_unit.GetActionDirection(), directionToPosition);
        float angle = Vector3.Angle(vectorFromAngle, directionToPosition);
        //if (Vector3.Angle(GetVectorFromAngle(_startingAngle), directionToPosition) > _fov / 2f)
        //if (Mathf.Abs(_startingAngle - GetAngleFromVectorFloat(directionToPosition)) > _fov / 2f)
        if (angle > _fov / 2f)
        {
            //Debug.Log("CanUnitSeeWorldPosition: " + this._unit.name + ": Position: " + position.ToString() + " is not in FOV: " + (_fov / 2f).ToString() + ":" + angle.ToString() + " starting angle: " + _startingAngle + " vector from starting angle: " + vectorFromAngle.ToString() + " unit action direction: " + _unit.GetActionDirection() + " angle from aim direction: " + GetAngleFromVectorFloat(_unit.GetActionDirection(), true));
            return false;
        }

        // Do a raycast to see if there is anything between the position and the unit that would block vision
        RaycastHit2D[] raycastHits2D = Physics2D.RaycastAll(unitPosition, directionToPosition, distanceToPosition);
        // if the ray hit nothing, the position can be seen
        if (raycastHits2D.Length == 0)
        {
            //Debug.Log("CanUnitSeeWorldPosition: " + this._unit.name + ":  can see Position: " + position.ToString() + " no colliders hit!");
            return true;
        }
            
        for (int i = 0; i < raycastHits2D.Length; i++)
        {
            if (raycastHits2D[i].collider.CompareTag("BombRunWall"))
            {
                return false;
            }
            if (raycastHits2D[i].collider.CompareTag("BombRunObstacle"))
            {
                if (raycastHits2D[i].transform.TryGetComponent<BaseBombRunObstacle>(out BaseBombRunObstacle obstacle))
                {
                    if (obstacle.IsWalkable() || obstacle.GetObstacleCoverType() != ObstacleCoverType.Full)
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            if (raycastHits2D[i].collider.gameObject.layer == _unitLayer)
            {
                continue;
            }
        }
        //Debug.Log("CanUnitSeeWorldPosition: " + this._unit.name + ":  can see Position: " + position.ToString());
        return true;

    }
    public bool CanUnitSeeGridPosition(GridPosition targetGridPosition)
    {
        //Debug.Log("CanUnitSeeGridPosition (GridPosition): " + targetGridPosition.ToString());
        Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(_unit.GetGridPosition());
        Vector3 targetWorldPosition = LevelGrid.Instance.GetWorldPosition(targetGridPosition);

        // check distance to position. Use GridPosition Distance so it matches how LevelGrid.GetGridPositionsInRadius does it
        int viewDistance = _unit.GetSightRange() * LevelGrid.Instance.GetPathFindingDistanceMultiplier();
        int distanceToGridPosition = LevelGrid.Instance.CalculateDistance(_unit.GetGridPosition(), targetGridPosition);
        if (distanceToGridPosition > viewDistance)
        {
            //Debug.Log("CanUnitSeeGridPosition: " + this._unit.name + ": Position: " + targetGridPosition.ToString() + " is too far to see");
            return false;
        }
        float distanceToWorldPosition = Vector2.Distance(unitWorldPosition, targetWorldPosition);
        // check the angle from the player to the position. If it is outside of FOV, return false
        Vector3 directionToPosition = (targetWorldPosition - unitWorldPosition).normalized;
        //Vector3 vectorFromAngle = GetVectorFromAngle(_aimDirectionAngle);
        Vector3 vectorFromAngle = _aimDirectionVector;
        float angle = Vector3.Angle(vectorFromAngle, directionToPosition);

        if ((int)angle > (int)(_fov / 2f))
        {
            //Debug.Log("CanUnitSeeGridPosition: " + this._unit.name + ": Position: " + targetGridPosition.ToString() + " is not in FOV: " + (_fov / 2f).ToString("0.000000") + ":" + angle.ToString("0.000000") + " starting angle: " + _startingAngle + " vector from starting angle: " + vectorFromAngle.ToString() + " unit action direction: " + _unit.GetActionDirection() + " angle from aim direction: " + GetAngleFromVectorFloat(_unit.GetActionDirection(), true));
            return false;
        }

        return DoesUnitHaveLineOfSightToPosition(unitWorldPosition, targetWorldPosition, directionToPosition, distanceToWorldPosition);
    }
    private bool DoesUnitHaveLineOfSightToPosition(Vector3 startPosition, Vector3 targetPosition,Vector3 direction, float distance)
    {

        // Do a raycast to see if there is anything between the position and the unit that would block vision
        RaycastHit2D[] raycastHits2D = Physics2D.RaycastAll(startPosition, direction, distance);
        // if the ray hit nothing, the position can be seen
        if (raycastHits2D.Length == 0)
        {
            return true;
        }

        for (int i = 0; i < raycastHits2D.Length; i++)
        {
            // I think this will let you see the grid position that a wall or obstacle is on? Before the Fog Of War would never be revealed on walls...
            if (Vector2.Distance(targetPosition, raycastHits2D[i].point) <= Mathf.Sqrt(LevelGrid.Instance.GetGridCellSize()))
            {
                return true;
            }
            if (raycastHits2D[i].collider.CompareTag("BombRunWall"))
            {
                return false;
            }
            if (raycastHits2D[i].collider.CompareTag("BombRunObstacle"))
            {
                if (raycastHits2D[i].transform.TryGetComponent<BaseBombRunObstacle>(out BaseBombRunObstacle obstacle))
                {
                    if (obstacle.IsWalkable() || obstacle.GetObstacleCoverType() != ObstacleCoverType.Full)
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            if (raycastHits2D[i].collider.gameObject.layer == _unitLayer)
            {
                continue;
            }
        }
        return true;
    }
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Goblin"))
    //    {
    //        BombRunUnit unit = collision.GetComponent<BombRunUnit>();
    //        if (unit.IsEnemy() != this._unit.IsEnemy())
    //        {
    //            //Debug.Log("BombRunUnitFieldOfView: OnTriggerEnter2D: Enemy unity spotted. Enemy unit: " + unit.name + " Spotter: " + this._unit.name) ;
    //            //UnitVisibilityManager_BombRun.Instance.AddUnitToVisibilityList(unit, this._unit);
    //            if (!_visibleUnits.Contains(unit))
    //            {
    //                //_visibleUnits.Add(unit);
    //            }
    //        }
    //    }
    //}
    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Goblin"))
    //    {
    //        BombRunUnit unit = collision.GetComponent<BombRunUnit>();
    //        if (unit.IsEnemy() != this._unit.IsEnemy())
    //        {
    //            //Debug.Log("BombRunUnitFieldOfView: OnTriggerExit2D: Enemy unity Left field of view. Enemy unit: " + unit.name + " Spotter: " + this._unit.name);
    //            //UnitVisibilityManager_BombRun.Instance.RemoveUnitFromVisibilityList(unit);
    //            //UnitVisibilityManager_BombRun.Instance.EnemyLeftObserverFOV(unit, this._unit);
    //            if (_visibleUnits.Contains(unit))
    //            {
    //                //_visibleUnits.Remove(unit);
    //            }
    //        }
    //    }
    //}
    private void Unit_OnThisUnitMovedGridPosition(object sender, EventArgs e)
    {
        GetVisibileGridPositions();
    }
    private void GetVisibileGridPositions()
    {
        // Get all grid positions in radius around unit
        List<GridPosition> gridRadius = LevelGrid.Instance.GetGridPositionsInRadius(_unit.GetGridPosition(), _unit.GetSightRange());

        List<GridPosition> previousVisibileGridPositions = new List<GridPosition>();
        previousVisibileGridPositions.AddRange(_visibleGridPositions);
        _visibleGridPositions.Clear();
        _visibleVector2Positions.Clear();
        if (_visibleUnits.Count > 0)
        {
            foreach (BombRunUnit unit in _visibleUnits)
            {
                UnitVisibilityManager_BombRun.Instance.EnemyLeftObserverFOV(unit, this._unit);
            }
        }

        _visibleUnits.Clear();
        _visibleGridPositions.Add(this._unit.GetGridPosition());
        //_visibleGridPositions.AddRange(GetAdjacentGridPositions(this._unit.GetGridPosition()));
        List<GridPosition> gridRadiusNotInView = new List<GridPosition>();
        //gridRadiusNotInView.AddRange(gridRadius);
        gridRadiusNotInView.AddRange(GetSurroundingGridPositions(this._unit.GetGridPosition()));
        foreach (GridPosition gridPosition in gridRadius)
        {

            bool canUnitSeePosition = CanUnitSeeGridPosition(gridPosition);

            if (canUnitSeePosition)
            {
                gridRadiusNotInView.Remove(gridPosition);
                _visibleGridPositions.Add(gridPosition);
                _visibleVector2Positions.Add(new Vector2(gridPosition.x, gridPosition.y));
                if (LevelGrid.Instance.HasAnyUnitOnGridPosition(gridPosition))
                {
                    BombRunUnit unit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
                    if (unit.IsEnemy() == this._unit.IsEnemy())
                        continue;

                    _visibleUnits.Add(unit);
                    UnitVisibilityManager_BombRun.Instance.AddUnitToVisibilityList(unit, this._unit);
                }
            }
        }
        //UnitVisibilityManager_BombRun.Instance.UpdateTeamsVisibleGridPositions(this._unit, _visibleGridPositions, previousVisibileGridPositions);
        UnitVisibilityManager_BombRun.Instance.UpdateTeamsVisibleGridPositions(this._unit, _visibleGridPositions, gridRadiusNotInView);
    }
    private List<GridPosition> GetAdjacentGridPositions(GridPosition gridPosition)
    {
        List<GridPosition> adjacentGridPositions = new List<GridPosition>();

        if (_aimDirectionVector == Vector2.right || _aimDirectionVector == Vector2.left)
        {
            GridPosition gridPosition1 = new GridPosition(gridPosition.x, gridPosition.y + 1);
            if (LevelGrid.Instance.IsValidGridPosition(gridPosition1))
            {
                adjacentGridPositions.Add(gridPosition1);
            }

            GridPosition gridPosition2 = new GridPosition(gridPosition.x, gridPosition.y - 1);
            if (LevelGrid.Instance.IsValidGridPosition(gridPosition2))
            {
                adjacentGridPositions.Add(gridPosition2);
            }
        }
        else if (_aimDirectionVector == Vector2.up || _aimDirectionVector == Vector2.down)
        {
            GridPosition gridPosition1 = new GridPosition(gridPosition.x + 1, gridPosition.y);
            if (LevelGrid.Instance.IsValidGridPosition(gridPosition1))
            {
                adjacentGridPositions.Add(gridPosition1);
            }

            GridPosition gridPosition2 = new GridPosition(gridPosition.x - 1, gridPosition.y);
            if (LevelGrid.Instance.IsValidGridPosition(gridPosition2))
            {
                adjacentGridPositions.Add(gridPosition2);
            }
        }
        else if (_aimDirectionVector == (new Vector2(1,1)).normalized || _aimDirectionVector == (new Vector2(-1, -1)).normalized)
        {
            GridPosition gridPosition1 = new GridPosition(gridPosition.x - 1, gridPosition.y + 1);
            if (LevelGrid.Instance.IsValidGridPosition(gridPosition1))
            {
                adjacentGridPositions.Add(gridPosition1);
            }

            GridPosition gridPosition2 = new GridPosition(gridPosition.x + 1, gridPosition.y - 1);
            if (LevelGrid.Instance.IsValidGridPosition(gridPosition2))
            {
                adjacentGridPositions.Add(gridPosition2);
            }
        }
        else if (_aimDirectionVector == (new Vector2(-1, 1)).normalized || _aimDirectionVector == (new Vector2(1, -1)).normalized)
        {
            GridPosition gridPosition1 = new GridPosition(gridPosition.x - 1, gridPosition.y - 1);
            if (LevelGrid.Instance.IsValidGridPosition(gridPosition1))
            {
                adjacentGridPositions.Add(gridPosition1);
            }

            GridPosition gridPosition2 = new GridPosition(gridPosition.x + 1, gridPosition.y + 1);
            if (LevelGrid.Instance.IsValidGridPosition(gridPosition2))
            {
                adjacentGridPositions.Add(gridPosition2);
            }
        }

        return adjacentGridPositions;
    }
    private List<GridPosition> GetSurroundingGridPositions(GridPosition gridPosition)
    {
        List<GridPosition> surroundingGridPositions = new List<GridPosition>();

        // From the player's starting position, cycle through the grid in the x and z axises and check if a valid grid position exists there.
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                GridPosition offsetGridPosition = new GridPosition(x, y);
                GridPosition testGridPosition = gridPosition + offsetGridPosition;
                surroundingGridPositions.Add(testGridPosition);
            }
        }
        return surroundingGridPositions;
    }
    public List<GridPosition> GetUnitsVisibileGridPositions()
    {
        return _visibleGridPositions;
    }
    private List<GridPosition> GetGridPositionsInViewCone(List<GridPosition> gridPositions, Vector2 aimDirection)
    {
        List<GridPosition> gridPositionsInViewCone = new List<GridPosition>();
        gridPositionsInViewCone.AddRange(gridPositions);

        SetAimDirection(_unit.GetActionDirection());
        Vector3 unitPosition = _unit.GetWorldPosition();

        foreach (GridPosition gridPosition in gridPositions)
        {
            // check the angle from the player to the position. If it is outside of FOV, return false
            Vector3 directionToPosition = (LevelGrid.Instance.GetWorldPosition(gridPosition) - unitPosition).normalized;
            Vector3 vectorFromAngle = GetVectorFromAngle(_aimDirectionAngle);
            //float angle = Vector3.Angle(_unit.GetActionDirection(), directionToPosition);
            float angle = Vector3.Angle(vectorFromAngle, directionToPosition);
        }

        return gridPositionsInViewCone;
    }
    private bool IsGridPositionInViewCone(GridPosition testGridPosition, float viewAngle, Vector3 startPosition, float fieldOfView )
    {
        bool isGridPositionInViewCone = true;

        Vector3 directionToPosition = (LevelGrid.Instance.GetWorldPosition(testGridPosition) - startPosition).normalized;
        Vector3 vectorFromAngle = GetVectorFromAngle(viewAngle);
        float angle = Vector3.Angle(vectorFromAngle, directionToPosition);

        if (angle > fieldOfView / 2f)
        {
            Debug.Log("IsGridPositionInViewCone: " + testGridPosition + " is outside of field of view");
            return false;
        }

        return isGridPositionInViewCone;
    }
    private bool CanUnitSeeGridPosition(GridPosition testGridPosition, Vector3 unitPosition)
    {
        bool canUnitSeeGridPosition = true;
        Vector3 testPosition = LevelGrid.Instance.GetWorldPosition(testGridPosition);
        Vector2 directionToPosition = (LevelGrid.Instance.GetWorldPosition(testGridPosition) - unitPosition).normalized;
        float distanceToPosition = Vector2.Distance(testPosition, unitPosition);

        // Do a raycast to see if there is anything between the position and the unit that would block vision
        RaycastHit2D[] raycastHits2D = Physics2D.RaycastAll(unitPosition, directionToPosition, distanceToPosition);
        // if the ray hit nothing, the position can be seen
        if (raycastHits2D.Length == 0)
        {
           return true;
        }

        for (int i = 0; i < raycastHits2D.Length; i++)
        {
            if (raycastHits2D[i].collider.CompareTag("BombRunWall"))
            {
                return false;
            }
            if (raycastHits2D[i].collider.gameObject.layer == _obstacleLayer)
            {
                if (raycastHits2D[i].transform.TryGetComponent<BaseBombRunObstacle>(out BaseBombRunObstacle obstacle))
                {
                    if (obstacle.IsWalkable() || obstacle.GetObstacleCoverType() != ObstacleCoverType.Full)
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            if (raycastHits2D[i].collider.gameObject.layer == _unitLayer)
            {
                continue;
            }
        }

        return canUnitSeeGridPosition;
    }
}
