using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ObstacleManager : NetworkBehaviour
{
    public static ObstacleManager instance;

    [Header("Obstacles")]
    [SerializeField] GameObject[] ObstaclePrefabs;
    [SerializeField] List<GameObject> ObstaclesToSpawn = new List<GameObject>();
    [SerializeField] List<GameObject> SpawnedObstacles = new List<GameObject>();

    [Header("No Place Zones")]
    [SerializeField] Vector3[] NoPlacePositions;
    [SerializeField] float minDistanceFromNoPlaceZones;
    [SerializeField] float minDistanceFromOtherObstacles;

    [Header("Field Boundaries")]
    [SerializeField] float minY; // -6 f
    [SerializeField] float maxY; // 5 f
    [SerializeField] float minX; // -37. 5
    [SerializeField] float maxX; // 38. 75 f

    private void Awake()
    {
        MakeInstance();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        GenerateObstacles();
    }
    // Get number of obstacles to spawn, then choose that number of obstacles randomly from the array ObstaclePrefabs
    [ServerCallback]
    public void GenerateObstacles()
    {
        Debug.Log("GenerateObstacles executed on server");
        if (!GameplayManager.instance.spawnObstaclesEnabled)
            return;
        var rng = new System.Random();
        //int numberToSpawn = rng.Next(4, 8);
        int numberToSpawn = rng.Next(7, 15);
        Debug.Log("GenerateObstacles: Number of obstacles to generate: " + numberToSpawn.ToString());
        for (int i = 0; i < numberToSpawn; i++)
        {
            ObstaclesToSpawn.Add(ObstaclePrefabs[rng.Next(ObstaclePrefabs.Length)]);
        }
        PlaceObstacles();
    }
    // After the obstacles have been selected, place them on the field
    [ServerCallback]
    void PlaceObstacles()
    {
        Debug.Log("PlaceObstacles executed on the server");
        if (ObstaclesToSpawn.Count == 0)
            return;
        for (int i = 0; i < ObstaclesToSpawn.Count; i++)
        {
            SpawnedObstacles.Add(Instantiate(ObstaclesToSpawn[i]));
            Vector3 newPosition = Vector3.zero;
            newPosition.x = Random.Range(minX, maxX);
            newPosition.y = Random.Range(minY, maxY);

            Debug.Log("PlaceObstacles: Location for obstacle " + SpawnedObstacles[i].gameObject.name + " is " + newPosition.ToString());
            SpawnedObstacles[i].transform.position = newPosition;
        }
        VerifyObstaclePosition();
        BalanceSideOfFieldObstaclesAreOn();
    }
    // AFter Obstacles have been spawned on the server, Double check to make sure the obstacles are not too close to any No Place zones or too close to each other
    [ServerCallback]
    void VerifyObstaclePosition()
    {
        Debug.Log("VerifyObstaclePosition executed on server");
        for (int i = 0; i < SpawnedObstacles.Count; i++)
        {
            Vector3 obstaclePosition = SpawnedObstacles[i].transform.position;
            bool tooClose = IsObstacleTooCloseToNoPlaceZones(SpawnedObstacles[i], i);
            if (tooClose)
            {
                while (tooClose)
                {
                    GetNewPositionForObstacle(SpawnedObstacles[i]);
                    tooClose = IsObstacleTooCloseToNoPlaceZones(SpawnedObstacles[i], i);
                }
            }
            NetworkServer.Spawn(SpawnedObstacles[i]);
        }
    }
    // Check distance of the obstacle from each of the no-place zones and the other obstacles. Return tooClose as true if the objects are too close to any of those
    [ServerCallback]
    bool IsObstacleTooCloseToNoPlaceZones(GameObject obstacle, int placeInSpawnedObstaclesArray)
    {
        Debug.Log("IsObstacleTooCloseToNoPlaceZones executed on the server");
        bool tooClose = false;
        Vector3 obstaclePosition = obstacle.transform.position;
        for (int i = 0; i < NoPlacePositions.Length; i++)
        {
            if (Vector3.Distance(obstaclePosition, NoPlacePositions[i]) <= minDistanceFromNoPlaceZones)
            {
                Debug.Log("IsObstacleTooCloseToNoPlaceZones: " + obstacle.name + " " + placeInSpawnedObstaclesArray.ToString() + " is too close to No Place Zone at position: " + NoPlacePositions[i].ToString());
                tooClose = true;
                break;
            }
        }
        if (!tooClose)
        {
            for (int i = 0; i < SpawnedObstacles.Count; i++)
            {
                if (i == placeInSpawnedObstaclesArray)
                    continue;
                if (Vector3.Distance(obstaclePosition, SpawnedObstacles[i].transform.position) <= minDistanceFromOtherObstacles)
                {
                    Debug.Log("IsObstacleTooCloseToNoPlaceZones: " + obstacle.name + " " + placeInSpawnedObstaclesArray.ToString() + " is too close to other obstacle " + SpawnedObstacles[i].gameObject.name + " at position: " + NoPlacePositions[i].ToString());
                    tooClose = true;
                    break;
                }
            }
        }
        return tooClose;
    }
    [ServerCallback]
    void GetNewPositionForObstacle(GameObject obstacle)
    {
        Debug.Log("GetNewPositionForObstacle executed on the server");
        Vector3 oldPosition = obstacle.transform.position;
        Vector3 newPosition = oldPosition;

        // Get the new position. Move on the X and Y a small random amount in a random direction
        float newY = Random.Range(1f, minDistanceFromNoPlaceZones);
        float newX = Random.Range(1f, minDistanceFromNoPlaceZones);
        int negOrPos = Random.Range(0, 2) * 2 - 1;
        newY *= negOrPos;
        negOrPos = Random.Range(0, 2) * 2 - 1;
        newX *= negOrPos;

        // Make sure that the new x/y values won't exceed the field boundaries
        if ((newPosition.x + newX) < minX)
            newX *= -1;
        if ((newPosition.x + newX) > maxX)
            newX *= -1;
        if ((newPosition.y + newY) < minY)
            newY *= -1;
        if ((newPosition.y + newY) > maxY)
            newY *= -1;

        newPosition.x += newX;
        newPosition.y += newY;

        Debug.Log("GetNewPositionForObstacle: Old position for obstacle " + obstacle.name + " was " + oldPosition.ToString() + " the new position will be: " + newPosition.ToString());
        obstacle.transform.position = newPosition;
    }
    // Try to balance out the obstacles on what side of the field they are on. It could be unfair if all or most objects are only on one side
    [ServerCallback]
    void BalanceSideOfFieldObstaclesAreOn()
    {
        Debug.Log("BalanceSideOfFieldObstaclesAreOn executed on the server");
        List<GameObject> leftSideOfField = new List<GameObject>();
        List<GameObject> rightSideOfField = new List<GameObject>();

        for (int i = 0; i < SpawnedObstacles.Count; i++)
        {
            if (SpawnedObstacles[i].transform.position.x < 0)
                leftSideOfField.Add(SpawnedObstacles[i]);
            else
                rightSideOfField.Add(SpawnedObstacles[i]);
        }

        Debug.Log("BalanceSideOfFieldObstaclesAreOn: Obstacles on the left: " + leftSideOfField.Count.ToString() + " Obstacles on the right side: " + rightSideOfField.Count.ToString() + " with total spawned obstacles being: " + SpawnedObstacles.Count);

        bool leftHasTooMuch = false;
        bool rightHasTooMuch = false;
        float leftSideRatio = (float)((float)leftSideOfField.Count / (float)SpawnedObstacles.Count);
        float rightSideRatio = (float)((float)rightSideOfField.Count / (float)SpawnedObstacles.Count);
        Debug.Log("BalanceSideOfFieldObstaclesAreOn: Ratio of obstacles on LEFT side of field: " + leftSideRatio.ToString() + " Ratio of obstacles on RIGHT side of field: " + rightSideRatio.ToString());
        if (leftSideRatio > 0.61f)
        {
            leftHasTooMuch = true;
            Debug.Log("BalanceSideOfFieldObstaclesAreOn: Too many obstacles on left side of field");

        }
        if (rightSideRatio > 0.61f)
        {
            rightHasTooMuch = true;
            Debug.Log("BalanceSideOfFieldObstaclesAreOn: Too many obstacles on right side of field");
        }
        if (leftHasTooMuch)
        {
            int difference = (leftSideOfField.Count - rightSideOfField.Count) / 2;
            Debug.Log("BalanceSideOfFieldObstaclesAreOn: Will move " + difference + " obstacles to the right side of the field");
            for (int i = 0; i < difference; i++)
            {
                int randomValueInList = Random.Range(0, leftSideOfField.Count);
                GameObject objectToFlip = leftSideOfField[randomValueInList];
                Vector3 newPosition = objectToFlip.transform.position;
                newPosition.x *= -1;
                objectToFlip.transform.position = newPosition;
                leftSideOfField.Remove(objectToFlip);
            }
        }
        else if (rightHasTooMuch)
        {
            int difference = (rightSideOfField.Count - leftSideOfField.Count) / 2;
            Debug.Log("BalanceSideOfFieldObstaclesAreOn: Will move " + difference + " obstacles to the left side of the field");
            for (int i = 0; i < difference; i++)
            {
                int randomValueInList = Random.Range(0, rightSideOfField.Count);
                GameObject objectToFlip = rightSideOfField[randomValueInList];
                Vector3 newPosition = objectToFlip.transform.position;
                newPosition.x *= -1;
                objectToFlip.transform.position = newPosition;
                rightSideOfField.Remove(objectToFlip);
            }
        }
    }
    // Disable colliders on obstacles during gamephases when players shouldn't be able to collide with them
    [ServerCallback]
    public void DisableCollidersOnObjects(bool enable)
    {
        for (int i = 0; i < SpawnedObstacles.Count; i++)
        {
            SpawnedObstacles[i].GetComponent<ObstacleObject>().DisableColliderDuringPhase(enable);
        }
    }
    [ServerCallback]
    public void KickAfterWaitToEnableObstacleColliders()
    {
        IEnumerator waitToEnableObstacleColliders = WaitToEnableObstacleColliders();
        StartCoroutine(waitToEnableObstacleColliders);
    }
    [ServerCallback]
    IEnumerator WaitToEnableObstacleColliders()
    {
        yield return new WaitForSeconds(0.666f);
        DisableCollidersOnObjects(true);
    }
}
