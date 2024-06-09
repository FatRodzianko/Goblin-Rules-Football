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

    [Header("Hole Type")]
    [SerializeField] TextMeshProUGUI _holeTypeText;
    string _regularHoleType = "Regular Golf Course:";
    string _miniHoleType = "Mini Golf Course:";

    [Header("Hole Number")]
    [SerializeField] TextMeshProUGUI _holeNumberValueText;

    [Header("Hole Par")]
    [SerializeField] TMP_InputField _holeParInput;

    [Header("Toggles")]
    [SerializeField] Toggle _hasHoleBeenPlaced;
    [SerializeField] Toggle _hasTeeOffLocationBeenSet;
    [SerializeField] Toggle _haveTeeMarkersBeenPlaced;

    [Header("Distance texts")]
    [SerializeField] GameObject _teeOffLocationText;
    [SerializeField] TextMeshProUGUI _teeOffLocationValue;
    [SerializeField] GameObject _holePositionText;
    [SerializeField] TextMeshProUGUI _holePositionValue;


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
        _builder.UpdateDistanceFromTeeOffToHoleChanged += SetDistanceToHole;
        _builder.AimPointsAddOrRemovedChanged += SetAimPointsAndDistances;
        _builder.TeeOffLocationUpdatedChanged += SetTeeOffLocationPosition;
        _builder.HolePositionUpdatedChanged += SetHolePosition;
        _mapMakerUIManager.IsCourseMiniGolfChanged += SetHoleTypeText;

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
        _builder.UpdateDistanceFromTeeOffToHoleChanged -= SetDistanceToHole;
        _builder.AimPointsAddOrRemovedChanged -= SetAimPointsAndDistances;
        _builder.TeeOffLocationUpdatedChanged -= SetTeeOffLocationPosition;
        _builder.HolePositionUpdatedChanged -= SetHolePosition;
        _mapMakerUIManager.IsCourseMiniGolfChanged -= SetHoleTypeText;

        // events for this guy
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
    void SetHoleTypeText(bool isMini)
    {
        Debug.Log("SetHoleTypeText: " + isMini);
        if (isMini)
            _holeTypeText.text = _miniHoleType;
        else
            _holeTypeText.text = _regularHoleType;
    }
    public void SetHoleNumberValue(int holeNumber)
    {
        _holeNumberValueText.text = holeNumber.ToString();
    }
    public void SetHasHoleBeenPlaced(bool placed)
    {
        Debug.Log("SetHasHoleBeenPlaced: " + placed);
        _hasHoleBeenPlaced.isOn = placed;
        _holePositionText.SetActive(placed);
        _holePositionValue.gameObject.SetActive(placed);
    }
    public void SetHasTeeOffLocationBeenPlaced(bool placed)
    {
        Debug.Log("SetHasTeeOffLocationBeenPlaced: " + placed);
        _hasTeeOffLocationBeenSet.isOn = placed;
        _teeOffLocationText.SetActive(placed);
        _teeOffLocationValue.gameObject.SetActive(placed);
    }
    public void SetHaveTeeMarkersBeenPlaced(bool placed)
    {
        Debug.Log("SetHaveTeeMarkersBeenPlaced: " + placed);
        _haveTeeMarkersBeenPlaced.isOn = placed;
    }
    public void SetDistanceToHole(float distance)
    {
        Debug.Log("SetDistanceToHole: " + distance);
        _distanceToHoleText.text = distance.ToString("0.##");
    }
    void SetAimPointsAndDistances(Dictionary<Vector3Int, float> aimPointsAndDistances)
    {
        Debug.Log("SetAimPointsAndDistances: aimPointsAndDistances.Count: " + aimPointsAndDistances.Count);

        if (aimPointsAndDistances.Count == 0)
        {
            foreach (AimPointPanelScript aimPointitems in _aimPointItems)
            {
                aimPointitems.gameObject.SetActive(false);
            }
            _aimPointHolder.SetActive(false);
            return;
        }

        _aimPointHolder.SetActive(true);
        int i = 0;
        foreach (KeyValuePair<Vector3Int, float> pointDistPair in aimPointsAndDistances)
        {
            _aimPointItems[i].gameObject.SetActive(true);
            _aimPointItems[i].SetAimPointPositionText(new Vector2(pointDistPair.Key.x, pointDistPair.Key.y));
            _aimPointItems[i].SetAimPointDistanceText(pointDistPair.Value);
            i++;
        }

        if (i < _aimPointItems.Count - 1)
        {
            for (int j = i; j < _aimPointItems.Count; j++)
            {
                _aimPointItems[j].gameObject.SetActive(false);
            }
        }
    }
    void SetHolePosition(Vector3Int holePosition)
    {
        _holePositionValue.text = "(" + holePosition.x.ToString("0.##") + "," + holePosition.y.ToString("0.##") + ")";
    }
    void SetTeeOffLocationPosition(Vector3Int teeOffLocationPosition)
    {
        _teeOffLocationValue.text = "(" + teeOffLocationPosition.x.ToString("0.##") + "," + teeOffLocationPosition.y.ToString("0.##") + ")";
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
