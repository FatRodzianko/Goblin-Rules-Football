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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDisable()
    {
        _builder.HasHoleBeenPlacedYetChanged -= SetHasHoleBeenPlaced;
        _builder.HasTeeOffLocationBeenPlacedYetChanged -= SetHasTeeOffLocationBeenPlaced;
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
}
