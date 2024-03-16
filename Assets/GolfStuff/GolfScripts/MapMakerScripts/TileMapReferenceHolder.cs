using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapReferenceHolder : MonoBehaviour
{
    [SerializeField] public List<Tilemap> AllMaps = new List<Tilemap>();
}
