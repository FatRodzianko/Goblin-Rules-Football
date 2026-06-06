using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Tilemaps;

[Serializable]
public class NoiseVisualIndicatorParameters
{
    public GridPosition NoiseGridPosition;
    public int NoiseDistance;

    public NoiseVisualIndicatorParameters(GridPosition noiseGridPosition, int noiseDistance)
    {
        NoiseGridPosition = noiseGridPosition;
        NoiseDistance = noiseDistance;
    }
}
public class NoiseVisualsTileMapManager : MonoBehaviour
{
    [Header("Tile Maps")]
    [SerializeField] private Tilemap _gridVisualTileMap;

    [Header("Tile Map Manager")]
    [SerializeField] private BombRunTileMapManager _bombRunTileMapManager;

    [Header("Fog of War")]
    [SerializeField] private FogOfWarTileMapManager _fogOfWarTileMapManager;

    [Header("Noise animation?")]
    [SerializeField] private float _animationDelay = 0.025f;
    [SerializeField] private int _maxNoiseDistance; // no need to draw noise visuals beyond what is on the screen

    [Header("Object Pool")]
    [SerializeField] private GameObject _gridNoiseVisualIndicatorObject;
    [SerializeField] private NoiseVisualObjectPool _noiseVisualObjectPool;

    private void Start()
    {
        BombRunUnit.OnAnyUnitActionMadeNoise += BombRunUnit_OnAnyUnitActionMadeNoise;
        // create the noise visual object pool
        _noiseVisualObjectPool = new NoiseVisualObjectPool(_gridNoiseVisualIndicatorObject, this.transform, 250, 500);
    }    

    private void OnDisable()
    {
        BombRunUnit.OnAnyUnitActionMadeNoise -= BombRunUnit_OnAnyUnitActionMadeNoise;
    }
    private void BombRunUnit_OnAnyUnitActionMadeNoise(object sender, ActionMadeNoiseEventArgs actionMadeNoiseArgs)
    {
        UnitActionMadeNoise(actionMadeNoiseArgs);
    }
    private void UnitActionMadeNoise(ActionMadeNoiseEventArgs actionMadeNoiseArgs)
    {
        // check if it was the local player's unit that made the noise?
        if (!BombRunUnitManager.Instance.IsUnitOwnedByPlayer(actionMadeNoiseArgs.NoiseMakingUnit))
        {
            return;
        }

        //StartActionMadeNoiseVisualsAsTask(actionMadeNoiseArgs.ActionGridPosition, actionMadeNoiseArgs.NoiseDistance);
        StartActionMadeNoiseVisuals(actionMadeNoiseArgs.ActionGridPosition, actionMadeNoiseArgs.NoiseDistance);
    }
    private async void StartActionMadeNoiseVisualsAsTask(GridPosition startingGridPosition, int noiseDistance)
    {
        await Task.Run(() => StartActionMadeNoiseVisuals(startingGridPosition, noiseDistance));
    }
    private void StartActionMadeNoiseVisuals(GridPosition startingGridPosition, int noiseDistance)
    {
        Debug.Log("StartActionMadeNoiseVisuals: Start position: " + startingGridPosition.ToString() + " noise distance: " + noiseDistance);
        List<GridPosition> noiseRadiusGridPositions = LevelGrid.Instance.GetGridPositionsInRadius(startingGridPosition, noiseDistance);
        if (noiseRadiusGridPositions.Count < 1)
            return;
        
        // Get a list of the grid positions that the noise will travel through and save grid distance
        int noisePathfindingDistance = noiseDistance * PathFinding.Instance.GetPathFindingDistanceMultiplier();
        //Dictionary<GridPosition, int> noiseGridPositionsByDistance = new Dictionary<GridPosition, int>();
        List<NoiseVisualIndicatorParameters> noiseGridPositionsByDistance = new List<NoiseVisualIndicatorParameters>();
        foreach (GridPosition noiseGridPosition in noiseRadiusGridPositions)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(LevelGrid.Instance.GetWorldPosition(noiseGridPosition));
            bool onScreen = screenPoint.x > 0f && screenPoint.x < Screen.width && screenPoint.y > 0f && screenPoint.y < Screen.height;
            if (!onScreen)
                continue;

            // if the tile is under fog of war, don't calculate path. Just show the "noise"
            // otherwise it could show player where walls/objects they haven't seen yet are?
            if (_fogOfWarTileMapManager.IsPositionInFogOfWar(noiseGridPosition))
            {
                int fogOfWarDistance = LevelGrid.Instance.CalculateDistance(startingGridPosition, noiseGridPosition) / PathFinding.Instance.GetPathFindingDistanceMultiplier();
                noiseGridPositionsByDistance.Add(new NoiseVisualIndicatorParameters(noiseGridPosition, fogOfWarDistance));
                //Debug.Log("StartActionMadeNoiseVisuals: Position in fog of war at: " + noiseGridPosition + " distance of: " + fogOfWarDistance);
                continue;
            }

            // check if sound can find a "path" to the grid radius position that does not exceed noise distance
            List<GridPosition> pathGridPositionList = PathFinding.Instance.FindPath(startingGridPosition, noiseGridPosition, out int pathLength, noisePathfindingDistance);
            if (pathLength > noisePathfindingDistance)
            {
                continue;
            }
            int noisePathDistance = pathLength / PathFinding.Instance.GetPathFindingDistanceMultiplier();
            noiseGridPositionsByDistance.Add(new NoiseVisualIndicatorParameters(noiseGridPosition, noisePathDistance));
        }

        if (noiseGridPositionsByDistance.Count < 1)
        {
            return;
        }

        //sort the dictionary by the distance?
        //noiseGridPositionsByDistance.OrderBy(x => x.NoiseDistance);
        //StartCoroutine(AnimateNoiseTiles(noiseGridPositionsByDistance.OrderBy(x => x.NoiseDistance).ToList(), noiseDistance * PathFinding.Instance.GetPathFindingDistanceMultiplier()));
        StartCoroutine(AnimateNoiseTiles(noiseGridPositionsByDistance.OrderBy(x => x.NoiseDistance).ToList(), noiseDistance));
        //_bombRunTileMapManager.AnimateAllNoiseTiles(noiseGridPositionsByDistance.OrderBy(x => x.NoiseDistance).ToList(), noiseDistance, _animationDelay);
        //_bombRunTileMapManager.AnimateAllNoiseTiles(noiseGridPositionsByDistance.OrderBy(x => x.NoiseDistance).ToList(), noiseDistance * PathFinding.Instance.GetPathFindingDistanceMultiplier(), _animationDelay);
    }
    IEnumerator AnimateNoiseTiles(List<NoiseVisualIndicatorParameters> noiseGridPositionsByDistance, int maxNoiseDistance)
    {
        if (noiseGridPositionsByDistance.Count < 1)
            yield break;
        if (maxNoiseDistance < 1)
            yield break;

        //Debug.Break();
        for (int i = 1; i <= maxNoiseDistance; i++)
        {
            var gridPositions = GenericPool<List<GridPosition>>.Get();
            gridPositions.Clear();
            gridPositions.AddRange(noiseGridPositionsByDistance.Where(x => x.NoiseDistance == i).Select(x => x.NoiseGridPosition).ToList());
            //List<GridPosition> gridPositions = noiseGridPositionsByDistance.Where(x => x.NoiseDistance == i).Select(x => x.NoiseGridPosition).ToList();
            if (gridPositions.Count < 1)
            {
                GenericPool<List<GridPosition>>.Release(gridPositions);
                continue;
            }

            //_bombRunTileMapManager.AnimateNoiseTiles(gridPositions);
            SpawnNoiseTiles(gridPositions);
            GenericPool<List<GridPosition>>.Release(gridPositions);
            yield return new WaitForSeconds(_animationDelay);
            //yield return new WaitForSeconds(0.15f);
        }        
    }
    public void SpawnNoiseTiles(List<GridPosition> gridPositions)
    {
        foreach (GridPosition gridPosition in gridPositions)
        {
            GameObject newObject = _noiseVisualObjectPool.GetObject(LevelGrid.Instance.GetWorldPosition(gridPosition));
            GridNoiseVisualIndicatorScript gridNoiseVisualIndicatorScript = newObject.GetComponent<GridNoiseVisualIndicatorScript>();
            gridNoiseVisualIndicatorScript.InitializeNoiseVisual(_noiseVisualObjectPool,_animationDelay, ReleaseFromPool);
            gridNoiseVisualIndicatorScript.StartAnimation();
        }
    }
    private void ReleaseFromPool(GameObject poolObject)
    {
        _noiseVisualObjectPool.ReleaseObject(poolObject);
    }
}
