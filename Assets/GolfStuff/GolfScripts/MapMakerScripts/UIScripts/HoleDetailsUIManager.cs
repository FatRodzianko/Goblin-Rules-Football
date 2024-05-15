using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class HoleDetailsUIManager : MonoBehaviour
{
    [SerializeField] MapMakerBuilder _builder;
    [SerializeField] MapMakerUIManager _mapMakerUIManager;
    [SerializeField] GameObject _editHoleDetailsPanel;

    [Header("Hole Details Panel Open")]
    [SerializeField] bool _isDetailsPanelOpen = false;
    public delegate void IsDetailsPanelOpenEvent(bool placed);
    public event IsDetailsPanelOpenEvent IsDetailsPanelOpenEventChanged;

    [Header("Hole Number")]
    [SerializeField] TextMeshProUGUI _holeNumberValueText;
    [Header("Hole Par")]
    [SerializeField] TMP_InputField _holeParInput;

    [Header("Toggles")]
    [SerializeField] Toggle _hasHoleBeenPlaced;
    [SerializeField] Toggle _hasTeeOffLocationBeenSet;
    [SerializeField] Toggle _haveTeeMarkersBeenPlaced;

    [Header("Hole Distance")]
    [SerializeField] TextMeshProUGUI _distanceToHoleText;

    [Header("Aim pont stuff")]
    [SerializeField] GameObject _aimPointHolder;
    [SerializeField] List<AimPointPanelScript> _aimPointItems = new List<AimPointPanelScript>();

    public bool IsDetailsPanelOpen
    {
        get
        {
            return _isDetailsPanelOpen;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (!_mapMakerUIManager)
            _mapMakerUIManager = this.GetComponent<MapMakerUIManager>();
        if (!_builder)
            _builder = MapMakerBuilder.GetInstance();

        // events?
        _builder.HasHoleBeenPlacedYetChanged += SetHasHoleBeenPlaced;
        _builder.HasTeeOffLocationBeenPlacedYetChanged += SetHasTeeOffLocationBeenPlaced;
        _builder.HaveBothTeeMarkersPlacedYetChanged += SetHaveTeeMarkersBeenPlaced;

        // events for this guy
        IsDetailsPanelOpenEventChanged += IsDetailsPanelOpenEventChangedFunction;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDisable()
    {
        _builder.HasHoleBeenPlacedYetChanged -= SetHasHoleBeenPlaced;
        _builder.HasTeeOffLocationBeenPlacedYetChanged -= SetHasTeeOffLocationBeenPlaced;
        _builder.HaveBothTeeMarkersPlacedYetChanged -= SetHaveTeeMarkersBeenPlaced;

        IsDetailsPanelOpenEventChanged -= IsDetailsPanelOpenEventChangedFunction;
    }
    public void SetHolePar(int par)
    {
        Debug.Log("SetHolePar: " + par);
        this._holeParInput.text = par.ToString();    
    }
    public int GetHolePar()
    {
        int result;
        if (Int32.TryParse(this._holeParInput.text, out result))
        {
            return result;
        }
        else
        {
            Debug.Log("GetHolePar: could not parse this._holeParInput.text to an integer");
            return 0;
        }
    }
    public void SetHoleNumberValue(int holeNumber)
    {
        _holeNumberValueText.text = holeNumber.ToString();
    }
    public void SetHasHoleBeenPlaced(bool placed)
    {
        Debug.Log("SetHasHoleBeenPlaced: " + placed);
        _hasHoleBeenPlaced.isOn = placed;
    }
    public void SetHasTeeOffLocationBeenPlaced(bool placed)
    {
        Debug.Log("SetHasTeeOffLocationBeenPlaced: " + placed);
        _hasTeeOffLocationBeenSet.isOn = placed;
    }
    public void SetHaveTeeMarkersBeenPlaced(bool placed)
    {
        Debug.Log("SetHaveTeeMarkersBeenPlaced: " + placed);
        _haveTeeMarkersBeenPlaced.isOn = placed;
    }
    public void SetDistanceToHole(float distance)
    {
        Debug.Log("SetDistanceToHole: " + distance);
        _distanceToHoleText.text = distance.ToString();
    }
    void IsDetailsPanelOpenEventChangedFunction(bool isOpen)
    {
        Debug.Log("IsDetailsPanelOpenEventChangedFunction: " + isOpen);
        _isDetailsPanelOpen = isOpen;
    }
    public void OpenDetailsPanelButtonPressed()
    {
        Debug.Log("OpenDetailsPanelButtonPressed: _isDetailsPanelOpen: " + _isDetailsPanelOpen.ToString());
        if (_isDetailsPanelOpen)
        {
            CloseDetailsPanel();            
        }
        else
        {
            OpenDetailsPanel();
        }

    }
    void OpenDetailsPanel()
    {
        _editHoleDetailsPanel.SetActive(true);
        //_isDetailsPanelOpen = true;
        IsDetailsPanelOpenEventChanged(true);
    }
    void CloseDetailsPanel()
    {
        _editHoleDetailsPanel.SetActive(false);
        //_isDetailsPanelOpen = false;
        IsDetailsPanelOpenEventChanged(false);
    }
}
