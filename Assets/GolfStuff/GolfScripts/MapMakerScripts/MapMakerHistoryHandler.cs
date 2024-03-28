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
        _mapMakerHistory.CanUndoChanged += SetUndoInteractable;
        _mapMakerHistory.CanRedoChanged += SetRedoInteractable;
    }
    private void Start()
    {
        
    }
    void SetUndoInteractable(bool interactable)
    {
        //Debug.Log("SetUndoInteractable: " + interactable);
        SetInteractable(interactable, _undoButton);
    }
    void SetRedoInteractable(bool interactable)
    {
        //Debug.Log("SetRedoInteractable: " + interactable);
        SetInteractable(interactable, _redoButton);
    }
    void SetInteractable(bool interactable, Button button)
    {
        if (button.interactable != interactable)
        {
            button.interactable = interactable;
        }
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
