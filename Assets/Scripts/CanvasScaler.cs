using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.U2D;
using UnityEngine.Experimental.Rendering.Universal;
using System;

public class CanvasScaler : MonoBehaviour
{
    public Camera MainCamera;
    [SerializeField] PixelPerfectCamera myPixelPerfectCamera;
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
        /*if (!myPixelPerfectCamera)
        {
            Debug.Log("AdjustScalingFactor: No pixel perfect camera for " + this.gameObject.name);
            return;
        }*/
        UnityEngine.UI.CanvasScaler c = GetComponent<UnityEngine.UI.CanvasScaler>();
        //   scaler.AdjustScalingFactor() = MainCamera.GetComponent<PixelPerfectCamera>().pixelRatio;
        try
        {
            c.scaleFactor = MainCamera.GetComponent<PixelPerfectCamera>().pixelRatio;
        }
        catch (Exception)
        {
            
        }
        
        //c.scaleFactor = myPixelPerfectCamera.pixelRatio;
        //Debug.Log("AdjustScalingFactor: pixel ration is " + myPixelPerfectCamera.pixelRatio.ToString() + " for " + this.gameObject.name);
        //c.scaleFactor = MainCamera.GetComponent<UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera>().pixelRatio;
        //c.scaleFactor = (MainCamera.GetComponent<PixelPerfectCamera>().pixelRatio) * 1.5f;
    }
}
