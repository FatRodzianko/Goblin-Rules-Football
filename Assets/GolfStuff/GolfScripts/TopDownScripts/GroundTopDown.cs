using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class GroundTopDown : MonoBehaviour
{
    [SerializeField] public int GroundLayerValue;
    [SerializeField] public string groundType;
    [SerializeField] public Vector2 slopeDirection;
    [SerializeField] public float slopeSpeedIncrease;
    [SerializeField] public Tilemap MyTiles;

    [Serializable]
    public struct TileSlopeDirectionAndSpeed
    {
        public Vector3Int TilePosition;
        public Vector2 SlopeDirection;
        public float SlopeSpeedModifier;
    }

    [SerializeField] public TileSlopeDirectionAndSpeed[] TilesWithSlope;
    [SerializeField] List<Vector3Int> _tilePositionsWithSlopes = new List<Vector3Int>();

    // Start is called before the first frame update
    void Awake()
    {
        if (!MyTiles)
            MyTiles = this.GetComponent<Tilemap>();


        /*if (TilesWithSlope.Length > 0)
        {
            SetDirectionalTiles();
        }*/
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /*void SetDirectionalTiles()
    {
        for (int i = 0; i < TilesWithSlope.Length; i++)
        {
            DirectionTileManager.instance.SetDirectiontile(TilesWithSlope[i].TilePosition, TilesWithSlope[i].SlopeDirection, TilesWithSlope[i].SlopeSpeedModifier);
            if(!_tilePositionsWithSlopes.Contains(TilesWithSlope[i].TilePosition))
                _tilePositionsWithSlopes.Add(TilesWithSlope[i].TilePosition);
        }
    }*/
    public Vector3Int GetTilePosition(Vector3 pos)
    { 
        return MyTiles.WorldToCell(pos);
    }
    /*public Tuple<Vector2, float> GetSlopeDirection(Vector3Int tilePos)
    {
        // Get the cell position that the ball is on
        //Vector3Int tileBallIsOn = MyTiles.WorldToCell(ballPos);

        // Check to see if that tile has a slope
        if (_tilePositionsWithSlopes.Contains(tilePos))
        {
            TileSlopeDirectionAndSpeed slopeToReturn = TilesWithSlope.FirstOrDefault(x => x.TilePosition == tilePos);
            return new Tuple<Vector2, float>(slopeToReturn.SlopeDirection, slopeToReturn.SlopeSpeedModifier);
        }
        else // If the tile position isn't in the list of tiles with slopes, return a direction of 0 and a speed modifier of 0f
        { 
            return new Tuple<Vector2, float>(Vector2.zero, 0f);
        }
    }*/
}
