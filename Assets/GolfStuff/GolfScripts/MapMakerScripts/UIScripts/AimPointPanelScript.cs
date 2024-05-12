using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AimPointPanelScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _positionText;
    [SerializeField] TextMeshProUGUI _distanceText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetAimPointPositionText(Vector2 position)
    {
        this._positionText.text = "(" + position.x.ToString() + " , " + position.y.ToString() + ")";
    }
    public void SetAimPointDistanceText(float distance)
    {
        this._distanceText.text = distance.ToString() + "m";
    }
}
