using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnitFieldOfView : MonoBehaviour
{
    [Header("FOV Parameters")]
    [SerializeField] private int _rayCount = 50;
    [SerializeField] private float _fov = 90f;
    [SerializeField] private MeshFilter _meshFilter;

    [Header("Raycast stuff?")]
    [SerializeField] private LayerMask _blockingLayers;
    private Vector3 _origin = Vector3.zero;
    private float _startingAngle = 0f;
    private Mesh _mesh;


    [SerializeField] private BombRunUnit _unit;
    private void Awake()
    {
        if (_meshFilter == null)
            this._meshFilter = this.GetComponent<MeshFilter>();
        if (_unit == null)
            this._unit = this.transform.parent.GetComponent<BombRunUnit>();
    }
    private void Start()
    {
        _mesh = new Mesh();
        _meshFilter.mesh = _mesh;

        //_startingAngle = GetAngleFromVectorFloat(new Vector3(-1, 0, 0));
    }
    private void LateUpdate()
    {
        

        // setup field of view parameters
        //Vector3 origin = Vector3.zero;
        _origin = _unit.transform.position;

        Vector3 dirToMouse = (LevelGrid.Instance.GetWorldPosition(MouseWorld.instance.GetCurrentMouseGridPosition()) - _origin).normalized;
        SetAimDirection(dirToMouse);
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
        for (int i = 0; i <= _rayCount; i++)
        {
            //Debug.Log("FieldOfView: Vertex index: " + vertexIndex.ToString() + " with max verticies: " + vertices.Length.ToString() + " for unit: " + _unit.name);
            Vector3 vertex;
            // raycast from origin and check for collisions
            RaycastHit2D raycastHit2D = Physics2D.Raycast(_origin, GetVectorFromAngle(angle), viewDistance, _blockingLayers);

            if (raycastHit2D.collider == null)
            {
                vertex = _origin + GetVectorFromAngle(angle) * viewDistance;
            }
            else
            {
                //Debug.Log("FieldOfView: raycast hit at: " + raycastHit2D.point.ToString() + " on " + raycastHit2D.collider.name);
                vertex = raycastHit2D.point;
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
            vertexIndex++;
            // increase angle for next loop. subtract to go clockwise
            angle -= angleIncrease;
        }

        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;

        this.transform.position = Vector3.zero;
    }
    public void SetOrigin(Vector3 origin)
    {
        this._origin = origin;
    }
    public void SetAimDirection(Vector3 aimDirection)
    {
        _startingAngle = GetAngleFromVectorFloat(aimDirection) + _fov / 2f;
    }
    public Vector3 GetVectorFromAngle(float angle)
    {
        // angle = 0 -> 360
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }
    public float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;
        return n;
    }
}
