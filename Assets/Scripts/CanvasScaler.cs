using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class CanvasScaler : MonoBehaviour
{
    public Camera MainCamera;
    // Start is called before the first frame update
    private void Awake()
    {
        //AdjustScalingFactor();
    }
    void Start()
    {        
        AdjustScalingFactor();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        AdjustScalingFactor();
    }
    void AdjustScalingFactor()
    {
        if (!MainCamera)
        {
            MainCamera = Camera.main;
        }
        UnityEngine.UI.CanvasScaler c = GetComponent<UnityEngine.UI.CanvasScaler>();
        //   scaler.AdjustScalingFactor() = MainCamera.GetComponent<PixelPerfectCamera>().pixelRatio;
        c.scaleFactor = MainCamera.GetComponent<PixelPerfectCamera>().pixelRatio;
        //c.scaleFactor = (MainCamera.GetComponent<PixelPerfectCamera>().pixelRatio) * 1.5f;
    }
}
