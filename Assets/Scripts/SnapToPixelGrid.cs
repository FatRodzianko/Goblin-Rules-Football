using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class SnapToPixelGrid : MonoBehaviour
{
    public PixelPerfectCamera ppc;

    private void LateUpdate()
    {
        transform.position = ppc.RoundToPixel(transform.position);
    }
}
