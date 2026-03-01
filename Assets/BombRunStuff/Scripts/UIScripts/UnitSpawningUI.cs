using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UnitSpawningUI : MonoBehaviour
{
    [SerializeField] private Button _startGameButton;

    public static event EventHandler OnUnitSpawnUIStartGameButtonPressed;

    private void Start()
    {
        _startGameButton.onClick.AddListener(() =>
        {
            StartGameButtonPressed();
        });

        BombRunUnitSpawner.OnSpawnLocationSelectedForAllPlayerUnits += BombRunUnitSpawner_OnSpawnLocationSelectedForAllPlayerUnits;
    }
    private void OnDisable()
    {
        BombRunUnitSpawner.OnSpawnLocationSelectedForAllPlayerUnits -= BombRunUnitSpawner_OnSpawnLocationSelectedForAllPlayerUnits;
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
}
