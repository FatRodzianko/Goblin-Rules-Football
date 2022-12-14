using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Projection : MonoBehaviour
{
    private Scene _simulationScene;
    private PhysicsScene2D _physicsScene;

    [SerializeField] private Transform _obstaclesParent;
    

    [Header("Line Drawing stuff")]
    [SerializeField] private LineRenderer _line;
    [SerializeField] private int _maxPhysicsFrameIterations;

    [SerializeField] LayerMask groundMask;
    

    // Start is called before the first frame update
    void Start()
    {
        CreatePhysicsScene();
    }
    void CreatePhysicsScene()
    {
        _simulationScene = SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics2D));
        _physicsScene = _simulationScene.GetPhysicsScene2D();

        foreach (Transform obj in _obstaclesParent)
        {
            var ghostObj = Instantiate(obj.gameObject, obj.position, obj.rotation);
            ghostObj.GetComponent<Renderer>().enabled = false;
            SceneManager.MoveGameObjectToScene(ghostObj, _simulationScene);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SimulateTrajectory(GolfBall golfBallPrefab, Vector3 pos, Vector2 velocity)
    {
        Debug.Log("SimulateTrajectory");
        var ghostObj = Instantiate(golfBallPrefab, pos, Quaternion.identity);
        ghostObj.GetComponent<Renderer>().enabled = false;
        SceneManager.MoveGameObjectToScene(ghostObj.gameObject, _simulationScene);


        ghostObj.HitBall(ghostObj.angle,ghostObj.power,ghostObj.spin);

        _line.positionCount = _maxPhysicsFrameIterations;
        for (int i = 0; i < _maxPhysicsFrameIterations; i++)
        {
            _physicsScene.Simulate(Time.fixedDeltaTime);
            Vector3 newPos = ghostObj.transform.position;
            _line.SetPosition(i, newPos);

            if (i > 0)
            {
                Vector3 previousPos = _line.GetPosition(i - 1);
                float circleRadius = ghostObj.GetComponent<CircleCollider2D>().radius;
                if (Physics2D.CircleCast(newPos, circleRadius, newPos - previousPos, circleRadius * 2f, groundMask))
                {
                    _line.positionCount = i;
                    break;
                }

            }
        }
        Destroy(ghostObj.gameObject);
    }
}
