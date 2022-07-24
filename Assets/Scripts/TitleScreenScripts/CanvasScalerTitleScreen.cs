using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class CanvasScalerTitleScreen : MonoBehaviour
{
    public Camera MainCamera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
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
        //c.scaleFactor = MainCamera.GetComponent<PixelPerfectCamera>().pixelRatio;
        //c.scaleFactor = (MainCamera.GetComponent<PixelPerfectCamera>().pixelRatio) * 1.5f;
        int screenHeight = Screen.height;
        switch (screenHeight)
        {
            case 720:
                c.scaleFactor = 2f;
                break;
            case 900:
                c.scaleFactor = 2.5f;
                break;
            case 1080:
                c.scaleFactor = 3f;
                break;
            case 1440:
                c.scaleFactor = 4f;
                break;
            case 2160:
                c.scaleFactor = 5f;
                break;
        }

    }
}
