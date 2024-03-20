using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapMakerHistoryHandler : MonoBehaviour
{
    [SerializeField] Button _undoButton, _redoButton;
    MapMakerHistory _mapMakerHistory;


    private void Awake()
    {
        _redoButton.onClick.AddListener(Redo);
        _undoButton.onClick.AddListener(Undo);

        _mapMakerHistory = MapMakerHistory.GetInstance();
    }
    void Undo()
    {
        _mapMakerHistory.UndoStep();
    }
    void Redo()
    {
        _mapMakerHistory.RedoStep();
    }
}
