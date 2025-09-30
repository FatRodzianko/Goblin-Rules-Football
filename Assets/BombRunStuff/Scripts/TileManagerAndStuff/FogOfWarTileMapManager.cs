using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FogOfWarTileMapManager : MonoBehaviour
{
    public static FogOfWarTileMapManager Instance { get; private set; }

    [Header("Tilemaps")]
    [SerializeField] private Tilemap _blackoutFogOfWarTileMap;
    [SerializeField] private Tilemap _greyedOutFogOfWarTileMap;


    private void Awake()
    {
        MakeInstance();
    }
    void MakeInstance()
    {
        if (Instance != null)
        {
            Debug.Log("MakeInstance: more than one FogOfWarTileMapManager. Destroying...");
            Destroy(this);
            return;
        }
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
