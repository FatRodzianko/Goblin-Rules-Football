using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapMakerToolController : MonoBehaviour
{
    public static MapMakerToolController instance;

    [SerializeField] List<Tilemap> _tileMaps = new List<Tilemap>();

    private void Awake()
    {
        MakeInstance();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    public void Eraser(Vector3Int position)
    {
        Debug.Log("MapMakerToolController: Eraser");


    }
}
