using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNoiseVisualIndicatorScript : MonoBehaviour
{
    public void AnimationEnded()
    {
        Destroy(this.gameObject);
    }
}
