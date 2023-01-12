using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Level Tile", menuName ="2D/Tiles/Level Tile")]
public class LevelTile : Tile
{
    public TileType Type;
}
public enum TileType
{ 
    // ground
    green = 0,
    fairway = 1,
    rough = 2,
    deepRough = 3,
    sandTrap = 4,
    waterTrap = 5,
    edges = 6,
    directionalTiles = 7
}
