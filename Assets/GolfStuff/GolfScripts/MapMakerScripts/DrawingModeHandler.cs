using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawingModeHandler : MonoBehaviour
{
    [SerializeField] List<DrawingModeButton> _drawingModeButtons = new List<DrawingModeButton>();
    [SerializeField] MapMakerBuilder _mapMakerBuilder;

    private void Awake()
    {
        _mapMakerBuilder = MapMakerBuilder.GetInstance();
    }
    private void Start()
    {
        // set the current drawing mode to single?
        ButtonClicked(PlaceType.Single);
        _mapMakerBuilder.SetDrawModeHandler(this);
    }
    public void ButtonClicked(PlaceType placeType)
    {
        Debug.Log("ButtonClicked: " + placeType);
        UpdateSelectedDrawingModeButton(placeType);
        _mapMakerBuilder.SetCurrentDrawingMode(placeType);
    }
    void UpdateSelectedDrawingModeButton(PlaceType placeType)
    {
        foreach (DrawingModeButton button in _drawingModeButtons)
        {
            if (button.PlaceType == placeType)
            {
                button.SetIfButtonIsSelected(true);
            }
            else
            {
                button.SetIfButtonIsSelected(false);
            }
        }
    }
    public void ResetToSingle()
    {
        ButtonClicked(PlaceType.Single);
    }
}
