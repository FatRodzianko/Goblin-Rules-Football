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
    private Mesh _mesh;

    [Header("Unit movement and other triggers for mesh creation")]
    [SerializeField] private bool _isMoving = false;

    [Header("FOV Materials and appearance")]
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private Material _friendlyFOVMaterial;
    [SerializeField] private Material _enemyFOVMaterial;

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
        if (_unit.TryGetComponent<MoveAction>(out MoveAction moveAction))
        {
            moveAction.OnStartMoving += MoveAction_OnStartMoving;
            moveAction.OnStopMoving += MoveAction_OnStopMoving;
        }

        LevelGrid.Instance.OnWallsAndFloorsPlacedCompleted += LevelGrid_OnWallsAndFloorsPlacedCompleted;

        //SetFOVMaterial(_unit.IsEnemy());
        //SetAimDirection(_unit.GetActionDirection());
        //UpdateFOVMesh();
    }

    

    private void OnDisable()
    {
        _unit.OnActionDirectionChanged -= Unit_OnActionDirectionChanged;
        if (_unit.TryGetComponent<MoveAction>(out MoveAction moveAction))
        {
            moveAction.OnStartMoving -= MoveAction_OnStartMoving;
            moveAction.OnStopMoving -= MoveAction_OnStopMoving;
        }
        LevelGrid.Instance.OnWallsAndFloorsPlacedCompleted -= LevelGrid_OnWallsAndFloorsPlacedCompleted;
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
        UpdateFOVMesh();
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
        UpdateFOVMesh();
    }

    private void LateUpdate()
    {

        if (!_isMoving)
            return;
        UpdateFOVMesh();
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
    private void UpdateFOVMesh()
    {
        // setup field of view parameters
        //Vector3 origin = Vector3.zero;
        _origin = _unit.transform.position;
        //_origin = LevelGrid.Instance.GetWorldPosition(_unit.GetGridPosition());

        // for testing: aim direction will be where mouse is relative to unit
        //Vector3 dirToMouse = (LevelGrid.Instance.GetWorldPosition(MouseWorld.instance.GetCurrentMouseGridPosition()) - _origin).normalized;
        //SetAimDirection(dirToMouse);
        float angle = _startingAngle;
        float angleIncrease = _fov / _rayCount;
        float viewDistance = _unit.GetSightRange() * LevelGrid.Instance.GetGridCellSize();

        // initialize the arrays
        Vector3[] vertices = new Vector3[_rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[_rayCount * 3];

        // set the origin
        vertices[0] = _origin;

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
                //RaycastHit2D raycastHit2D = Physics2D.Raycast(_origin, vectorFromAngle, viewDistance, layerMask);
                RaycastHit2D[] raycastHits2D = Physics2D.RaycastAll(_origin, vectorFromAngle, viewDistance, layerMask);
                //if (raycastHits2D.collider == null)
                //    continue;
                if (raycastHits2D.Length == 0)
                    continue;
                //if (layerMask == _obstacleLayer)
                //{
                //    if (raycastHit2D.transform.TryGetComponent<BaseBombRunObstacle>(out BaseBombRunObstacle obstacle))
                //    {
                //        if (obstacle.IsWalkable() || obstacle.GetObstacleCoverType() != ObstacleCoverType.Full)
                //        {
                //            continue;
                //        }
                //    }
                //}
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

                //float newDistance = Vector2.Distance(_origin, raycastHits2D.point);
                //if (firstIteration)
                //{
                //    closestHitDistance = newDistance;
                //    vertex = raycastHits2D.point;
                //    firstIteration = false;
                //    continue;
                //}
                //if (newDistance <= closestHitDistance)
                //{
                //    closestHitDistance = newDistance;
                //    vertex = raycastHits2D.point;
                //}

            }

            vertices[vertexIndex] = vertex;

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
            if (_frameCounter > 10 || !_isMoving)
                spottedEnemyUnits.AddRange(FindVisibleEnemyUnits(spottedEnemyUnits, _origin, vectorFromAngle, Vector2.Distance(_origin, vertex) - 0.1f)); // the -0.1f is to make it so the ray won't go all the way to this hit object? In event unit and obstacle are overlapping at exact point? idk
            vertexIndex++;
            // increase angle for next loop. subtract to go clockwise
            angle -= angleIncrease;
        }

        // this ends up getting called way to often. Maybe have it so it's only called every so often?
        // Have a list of units spotted while moving. while isMoving is true, check to see if you spotted any new units. If so, add to new unit list, and update the UnitVisibilityManager with just that one unit?
        // when the unit stops moving, then submit to UnitCompletedFOVCheck? So it's only happening
        //UnitVisibilityManager_BombRun.Instance.UnitCompletedFOVCheck(this._unit, spottedEnemyUnits);
        if (_frameCounter > 10 || !_isMoving)
        {
            UnitVisibilityManager_BombRun.Instance.UnitCompletedFOVCheck(this._unit, spottedEnemyUnits);
            _frameCounter = 0;
        }
        _frameCounter++;

        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;
        _mesh.bounds = new Bounds(_origin, Vector3.one * 500f);

        this.transform.position = Vector3.zero;
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
        if(!_isMoving)
            UpdateFOVMesh();
    }
    public void SetAimDirection(Vector3 aimDirection)
    {
        _startingAngle = GetAngleFromVectorFloat(aimDirection.normalized, true) + _fov / 2f;
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
    public Vector3 GetEightDirectionVector(Vector3 dir)
    {
        Vector3 newDirection = Vector3.zero;
        if (dir.y > 0)
        {
            
        }
        return newDirection;
    }
    public bool CanUnitSeePosition(Vector3 position)
    {
        Vector3 unitPosition = _unit.transform.position;
        //Vector3 unitPosition = LevelGrid.Instance.GetWorldPosition(_unit.GetGridPosition());

        // check distance to position. If it is greater than the unit's view distance, return false
        float viewDistance = _unit.GetSightRange() * LevelGrid.Instance.GetGridCellSize();
        float distanceToPosition = Vector2.Distance(unitPosition, position);
        if (distanceToPosition > viewDistance)
        {
            Debug.Log("CanUnitSeePosition: " + this._unit.name + ": Position: " + position.ToString() + " is too far to see");
            return false;
        }

        // check the angle from the player to the position. If it is outside of FOV, return false
        Vector3 directionToPosition = (position - unitPosition).normalized;
        SetAimDirection(_unit.GetActionDirection());
        //Vector3 vectorFromAngle = GetVectorFromAngle(_startingAngle);
        Vector3 vectorFromAngle = GetVectorFromAngle(GetAngleFromVectorFloat(_unit.GetActionDirection(), true));
        //float angle = Vector3.Angle(_unit.GetActionDirection(), directionToPosition);
        float angle = Vector3.Angle(vectorFromAngle, directionToPosition);
        //if (Vector3.Angle(GetVectorFromAngle(_startingAngle), directionToPosition) > _fov / 2f)
        //if (Mathf.Abs(_startingAngle - GetAngleFromVectorFloat(directionToPosition)) > _fov / 2f)
        if (angle > _fov / 2f)
        {
            Debug.Log("CanUnitSeePosition: " + this._unit.name + ": Position: " + position.ToString() + " is not in FOV: " + (_fov / 2f).ToString() + ":" + angle.ToString() + " starting angle: " + _startingAngle + " vector from starting angle: " + vectorFromAngle.ToString() + " unit action direction: " + _unit.GetActionDirection() + " angle from aim direction: " + GetAngleFromVectorFloat(_unit.GetActionDirection(), true));
            return false;
        }

        // Do a raycast to see if there is anything between the position and the unit that would block vision
        RaycastHit2D[] raycastHits2D = Physics2D.RaycastAll(unitPosition, directionToPosition, distanceToPosition);
        // if the ray hit nothing, the position can be seen
        if (raycastHits2D.Length == 0)
        {
            Debug.Log("CanUnitSeePosition: " + this._unit.name + ":  can see Position: " + position.ToString() + " no colliders hit!");
            return true;
        }
            
        for (int i = 0; i < raycastHits2D.Length; i++)
        {
            if (raycastHits2D[i].collider.CompareTag("BombRunWall"))
            {
                Debug.Log("CanUnitSeePosition: " + this._unit.name + ": Position: " + position.ToString() + " is a wall...");
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
                }
            }
            if (raycastHits2D[i].collider.gameObject.layer == _unitLayer)
            {
                continue;
            }
        }
        Debug.Log("CanUnitSeePosition: " + this._unit.name + ":  can see Position: " + position.ToString());
        return true;

    }
}
