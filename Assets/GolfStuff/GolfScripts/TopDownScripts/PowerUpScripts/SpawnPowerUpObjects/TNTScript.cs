using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet;

public class TNTScript : NetworkBehaviour
{
    [SerializeField] [SyncVar] public int PlayerConnectionID; // connection id of the player object that spawned this TNT
    [SerializeField] [SyncVar] float _radius;

    [Header("Line Radius Stuff")]
    [SerializeField] LineRadiusDrawer _lineRadiusDrawer;

    // Start is called before the first frame update
    void Start()
    {
        UpdateRadius(_radius);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void UpdateRadius(float newRadius)
    {
        _lineRadiusDrawer.DrawRadius(newRadius);
    }
}
