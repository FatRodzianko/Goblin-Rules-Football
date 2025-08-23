using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRunUnitFieldOfView : MonoBehaviour
{
    [SerializeField] private MeshFilter _meshFilter;
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
        Mesh mesh = new Mesh();
        _meshFilter.mesh = mesh;

        // setup field of view parameters
        float fov = 90f;
        Vector3 origin = Vector3.zero;
        int rayCount = 2;
        float angle = 0f;
        float angleIncrease = fov / rayCount;
        float viewDistance = _unit.GetSightRange() * 2;

        // initialize the arrays
        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        // set the origin
        vertices[0] = origin;

        // cycle through rays and add new vertex
        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= rayCount; i++)
        {
            Debug.Log("FieldOfView: Vertex index: " + vertexIndex.ToString() + " with max verticies: " + vertices.Length.ToString() + " for unit: " + _unit.name);
            Vector3 vertex = origin + GetVectorFromAngle(angle) * viewDistance;
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

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }
    public Vector3 GetVectorFromAngle(float angle)
    {
        // angle = 0 -> 360
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }
}
