using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawingModeButton : MonoBehaviour
{
    [SerializeField] Button _button;
    [SerializeField] PlaceType _placeType = PlaceType.Single;
    [SerializeField] DrawingModeHandler _drawingModeHandler;
    [SerializeField] Sprite _defaultImage;
    [SerializeField] Sprite _selectedImage;

    // Start is called before the first frame update
    private void Awake()
    {
        if (!_button)
        {
            _button = this.GetComponent<Button>();
        }
        if (!_drawingModeHandler)
            return;
        _button.onClick.AddListener(ButtonClicked);

    }
    public PlaceType PlaceType
    {
        get
        {
            return _placeType;
        }
    }
    void ButtonClicked()
    {
        Debug.Log("Button was clicked: " + this.name);
        _drawingModeHandler.ButtonClicked(this._placeType);
    }
    public void SetIfButtonIsSelected(bool isSelected)
    {
        if (isSelected)
        {
            this._button.image.sprite = _selectedImage;
        }
        else
        {
            this._button.image.sprite = _defaultImage;
        }
        
    }
}
