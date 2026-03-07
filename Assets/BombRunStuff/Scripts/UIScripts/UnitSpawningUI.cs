using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
public class UnitSpawningUI : MonoBehaviour
{
    [SerializeField] private Button _startGameButton;
    [SerializeField] private GameObject _unitPortraitHolder;

    public static event EventHandler OnUnitSpawnUIStartGameButtonPressed;

    [SerializeField] private Transform _unitPortraitUIButtonPrefab;
    [SerializeField] private List<UnitSpawningButtonUI> _unitSpawningButtonUIObjects = new List<UnitSpawningButtonUI>();

    private void Start()
    {
        _startGameButton.onClick.AddListener(() =>
        {
            StartGameButtonPressed();
        });

        BombRunUnitSpawner.OnSpawnLocationSelectedForAllPlayerUnits += BombRunUnitSpawner_OnSpawnLocationSelectedForAllPlayerUnits;
        BombRunUnitSpawner.OnCreateUIObjectForUnitToSpawn += BombRunUnitSpawner_OnCreateUIObjectForUnitToSpawn;
        BombRunUnitSpawner.OnSpawnedUnitAtIndex += BombRunUnitSpawner_OnSpawnedUnitAtIndex;
    }
    private void OnDisable()
    {
        BombRunUnitSpawner.OnSpawnLocationSelectedForAllPlayerUnits -= BombRunUnitSpawner_OnSpawnLocationSelectedForAllPlayerUnits;
        BombRunUnitSpawner.OnCreateUIObjectForUnitToSpawn -= BombRunUnitSpawner_OnCreateUIObjectForUnitToSpawn;
        BombRunUnitSpawner.OnSpawnedUnitAtIndex -= BombRunUnitSpawner_OnSpawnedUnitAtIndex;
    }

    

    private void BombRunUnitSpawner_OnSpawnLocationSelectedForAllPlayerUnits(object sender, EventArgs e)
    {
        _startGameButton.gameObject.SetActive(true);
    }
    private void StartGameButtonPressed()
    {
        OnUnitSpawnUIStartGameButtonPressed?.Invoke(this, EventArgs.Empty);
        _startGameButton.gameObject.SetActive(false);
    }
    private void BombRunUnitSpawner_OnCreateUIObjectForUnitToSpawn(object sender, OnCreateUIObjectForUnitToSpawnEventArgs args)
    {
        Transform unitPortraitUIButtonTransform = Instantiate(_unitPortraitUIButtonPrefab, _unitPortraitHolder.transform);
        UnitSpawningButtonUI unitSpawningButtonUI = unitPortraitUIButtonTransform.GetComponent<UnitSpawningButtonUI>();

        unitSpawningButtonUI.InitializeUIObject(args.ScriptableBombRunUnit.UnitPortrait(), args.Index, args.ScriptableBombRunUnit.UnitType().ToString());

        _unitSpawningButtonUIObjects.Add(unitSpawningButtonUI);
    }
    private void BombRunUnitSpawner_OnSpawnedUnitAtIndex(object sender, int index)
    {
        UnitSpawningButtonUI unitSpawningButtonUI = _unitSpawningButtonUIObjects.First(x => x.GetIndex() == index);
        if (unitSpawningButtonUI == null)
            return;

        _unitSpawningButtonUIObjects.Remove(unitSpawningButtonUI);
        GameObject.Destroy(unitSpawningButtonUI.gameObject);
    }
}
