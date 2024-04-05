using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimizeMaximizeManager : MonoBehaviour
{
    [Header("MinMax Button")]
    [SerializeField] Button _minMaxButton;
    [SerializeField] Sprite _minimizeIcon;
    [SerializeField] Sprite _maximizeIcon;

    [Header("Building Overlay UI")]
    [SerializeField] RectTransform _buildingOverlay;
    [SerializeField] float _yPos;

    [Header("Misc.")]
    [SerializeField] bool _isMinimized = false;

    [SerializeField] MapMakerBuilder _mapMakerBuilder;
    // Start is called before the first frame update
    void Start()
    {
        _minMaxButton.onClick.AddListener(MinMaxButtonClicked);
        _yPos = _buildingOverlay.anchoredPosition.y;
        if (_mapMakerBuilder == null)
            _mapMakerBuilder = MapMakerBuilder.GetInstance();

        _mapMakerBuilder.SetMinMaxManager(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void MinMaxButtonClicked()
    {
        if (_isMinimized)
        {
            Debug.Log("MinMaxButtonClicked: Will MAXimize the window.");
            // resetting the window to be maximize, so change icon back to the minimize icon
            _minMaxButton.image.sprite = _minimizeIcon;

            SetYPositionOfOverlay(true);
            _isMinimized = false;
        }
        else
        {
            Debug.Log("MinMaxButtonClicked: Will MINimize the window.");
            // resetting the window to be minimize, so change icon back to the maximize icon
            _minMaxButton.image.sprite = _maximizeIcon;

            SetYPositionOfOverlay(false);
            _isMinimized = true;
        }
    }
    void SetYPositionOfOverlay(bool isPositiveValue)
    {
        Vector3 newPos = _buildingOverlay.anchoredPosition;
        if (isPositiveValue)
            newPos.y = _yPos;
        else
            newPos.y = -_yPos;
        _buildingOverlay.anchoredPosition = newPos;
    }
    public void MaximizeShortCut()
    {
        if (_isMinimized)
            MinMaxButtonClicked();
    }
    public void MinimizeShortCut()
    {
        if(!_isMinimized)
            MinMaxButtonClicked();
    }
}
