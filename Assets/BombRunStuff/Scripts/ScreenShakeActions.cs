using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShakeActions : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GrenadeProjectile.OnAnyGrenadeExploded += GrenadeProjectile_OnAnyGrenadeExploded;
    }
    private void OnDisable()
    {
        GrenadeProjectile.OnAnyGrenadeExploded -= GrenadeProjectile_OnAnyGrenadeExploded;
    }
    private void GrenadeProjectile_OnAnyGrenadeExploded(object sender, EventArgs e)
    {
        ScreenShake.Instance.Shake(5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
