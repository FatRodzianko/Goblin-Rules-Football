using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitNoiseHearingManager : MonoBehaviour
{
    public static event EventHandler<BombRunUnit> OnAnyUnitWasHeard;
    // Start is called before the first frame update
    void Start()
    {
        BombRunUnit.OnAnyUnitActionMadeNoise += BombRunUnit_OnAnyUnitActionMadeNoise;
    }    

    private void OnDisable()
    {
        BombRunUnit.OnAnyUnitActionMadeNoise -= BombRunUnit_OnAnyUnitActionMadeNoise;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void BombRunUnit_OnAnyUnitActionMadeNoise(object sender, ActionMadeNoiseEventArgs actionMadeNoiseArguments)
    {
        Debug.Log("BombRunUnit_OnAnyUnitActionMadeNoise: ");
        BombRunUnit unitMakingNoise = actionMadeNoiseArguments.NoiseMakingUnit;
        if (unitMakingNoise.GetUnitVisibility())
        {
            return;
        }
        CheckIfAnyUnitsHeardAction(unitMakingNoise, actionMadeNoiseArguments.ActionGridPosition, actionMadeNoiseArguments.NoiseDistance);
    }
    private void CheckIfAnyUnitsHeardAction(BombRunUnit unitMakingNoise, GridPosition noisePosition, int noiseDistance)
    {
        List<BombRunUnit> unitsToCheck = GetUnitsEnemies(unitMakingNoise);
        List<BombRunUnit> unitsCloseEnoughToHear = new List<BombRunUnit>();
        foreach (BombRunUnit unit in unitsToCheck)
        {
            if (IsUnitCloseEnoughToHearNoise(noisePosition, unit.GetGridPosition(), noiseDistance, unit.GetHearingSensitivity()))
            {
                //unitsCloseEnoughToHear.Add(unit);
                Debug.Log("CheckIfAnyUnitsHeardAction: " + unit.name + " heard " + unitMakingNoise.name + " take their action.");
                OnAnyUnitWasHeard?.Invoke(this, unitMakingNoise);
                break;
            }
        }

        //foreach (BombRunUnit unit in unitsCloseEnoughToHear)
        //{
        //    if (CanSoundTravelToListeningUnit())
        //    {
        //        break;
        //    }
        //}
    }
    private bool IsUnitCloseEnoughToHearNoise(GridPosition noiseStartPosition, GridPosition listeningUnitPosition, int noiseDistance, float listeningUnitHearingSensitivity)
    {
        
        int distance = LevelGrid.Instance.CalculateDistance(noiseStartPosition, listeningUnitPosition);
        int hearingDistance = (int)(noiseDistance * listeningUnitHearingSensitivity * LevelGrid.Instance.GetPathFindingDistanceMultiplier());
        Debug.Log("IsUnitCloseEnoughToHearNoise: noiseStartPosition: " + noiseStartPosition + " listeningUnitPosition: " + listeningUnitPosition + " noiseDistance: " + noiseDistance + " hearing sensitivity: " + listeningUnitHearingSensitivity + " hearingDistance: " + hearingDistance + " distance between noise source and listening unit: " + distance);
        if (distance <= hearingDistance)
        {
            // check if sound can find a "path" to the unit that does not exceed hearing distance
            List<GridPosition> pathGridPositionList = PathFinding.Instance.FindPath(noiseStartPosition, listeningUnitPosition, out int pathLength, hearingDistance);
            if (pathGridPositionList == null)
            {
                return false;
            }
            if (pathLength <= hearingDistance)
            {
                return true;
            }
        }
        return false;
    }
    //private bool CanSoundTravelToListeningUnit(GridPosition noiseStartPosition, GridPosition listeningUnitPosition, int noiseDistance, float listeningUnitHearingSensitivity)
    //{
    //    int maxDistance = 
    //    // Get the path to the end position
        
    //}
    private List<BombRunUnit> GetUnitsTeammates(BombRunUnit unit)
    {
        if (unit.IsEnemy())
        {
            return BombRunUnitManager.Instance.GetEnemyUnitList();
        }
        else
        {
            return BombRunUnitManager.Instance.GetFriendlyUnitList();
        }
    }
    private List<BombRunUnit> GetUnitsEnemies(BombRunUnit unit)
    {
        if (unit.IsEnemy())
        {
            return BombRunUnitManager.Instance.GetFriendlyUnitList();
        }
        else
        {
            return BombRunUnitManager.Instance.GetEnemyUnitList();
        }
    }
}
