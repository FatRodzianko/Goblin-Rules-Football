using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CowboyManager : NetworkBehaviour
{
    public static CowboyManager instance;
    IEnumerator cowboyGeneartor;
    bool isCowboyGeneratorRunning;

    [Header("The Cowboy")]
    [SerializeField] GameObject goblinCowboyPrefab;
    [SerializeField] GameObject goblinCowboyObject;
    [SerializeField] Vector3[] spawnPoints;
	[SerializeField] float minXCowboyDestination;
	[SerializeField] float maxXCowboyDestination;
	[SerializeField] float yCowboyDestination;

	public bool isCowboySpawned = false;


    // Start is called before the first frame update
    private void Awake()
    {
        MakeInstance();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	[ServerCallback]
	public void StartGeneratingCowboys(bool generate)
	{
		Debug.Log("StartGeneratingCowboys: " + generate.ToString());
		if (GameplayManager.instance.gamePhase == "gameplay" && generate && !isCowboyGeneratorRunning)
		{
			Debug.Log("StartGeneratingCowboys: starting GenerateCowboysRoutine");
			cowboyGeneartor = GenerateCowboysRoutine();
			StartCoroutine(cowboyGeneartor);
		}
		if (!generate)
		{
			Debug.Log("StartGeneratingCowboys: Stopping GenerateCowboysRoutine");
			isCowboyGeneratorRunning = false;
			StopCoroutine(cowboyGeneartor);
		}
	}
	[ServerCallback]
	IEnumerator GenerateCowboysRoutine()
	{
		isCowboyGeneratorRunning = true;
		float randomWaitTime;
		while (isCowboyGeneratorRunning)
		{
			randomWaitTime = Random.Range(0.75f, 15.5f);
			yield return new WaitForSeconds(randomWaitTime);
			CheckIfACowboySpawns();
			//yield break; 
		}
		//yield return new WaitForSeconds(1.0f);
		yield break;
	}
	[ServerCallback]
	void CheckIfACowboySpawns()
	{
		if (!isCowboySpawned)
		{
			bool willCowboySpawn = false;
			if (Random.Range(0f, 1f) > 0.2f)
				willCowboySpawn = true;
			if (willCowboySpawn)
			{
				// Get the spawn and exit points of the cowboy
				var rng = new System.Random();
				Vector3 cowboySpawnPoint = spawnPoints[rng.Next(spawnPoints.Length)];
				Vector3 cowboyExitPoint = spawnPoints[rng.Next(spawnPoints.Length)];

				// Get the destination point of the cowboy
				float xDestination = Random.Range(minXCowboyDestination, maxXCowboyDestination);
				Vector3 cowboyDestinationPoint = new Vector3(xDestination, yCowboyDestination, 0f);

				// Spawn the cowboy
				goblinCowboyObject = Instantiate(goblinCowboyPrefab);
				goblinCowboyObject.transform.position = cowboySpawnPoint;
				NetworkServer.Spawn(goblinCowboyObject);
				isCowboySpawned = true;

				// Set the cowboy scripts paramaters
				CowboyScript cowboyScript = goblinCowboyObject.GetComponent<CowboyScript>();
				cowboyScript.spawnPoint = cowboySpawnPoint;
				cowboyScript.destinationPoint = cowboyDestinationPoint;
				cowboyScript.exitPoint = cowboyExitPoint;
				cowboyScript.destinationY = yCowboyDestination;
				if (cowboySpawnPoint.x > cowboyDestinationPoint.x)
				{
					cowboyScript.xDirectionOfDestination = -1f;
				}
				else if (cowboySpawnPoint.x < cowboyDestinationPoint.x)
				{
					cowboyScript.xDirectionOfDestination = 1f;
				}
				if (cowboyExitPoint.x > cowboyDestinationPoint.x)
				{
					cowboyScript.xDirectiontoExit = 1f;
				}
				else if (cowboyExitPoint.x < cowboyDestinationPoint.x)
				{
					cowboyScript.xDirectiontoExit = -1f;
				}
			}
		}
	}
}
