using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class DirectionTileManager : MonoBehaviour
{
    public static DirectionTileManager instance;

    [SerializeField] Tilemap _directionTiles;

    [Header("Slope Speed Values")]
    [SerializeField] float _slowSpeed = 2f;
    [SerializeField] float _medSpeed = 5f;
    [SerializeField] float _fastSpeed = 10f;

    [Header("Fast Tiles")]
    [SerializeField] Tile _fastNorth;
    [SerializeField] Tile _fastNorthEast;
    [SerializeField] Tile _fastNorthWest;
    [SerializeField] Tile _fastSouth;
    [SerializeField] Tile _fastSouthEast;
    [SerializeField] Tile _fastSouthWest;
    [SerializeField] Tile _fastEast;
    [SerializeField] Tile _fastWest;

    [Header("Medium Tiles")]
    [SerializeField] Tile _medNorth;
    [SerializeField] Tile _medNorthEast;
    [SerializeField] Tile _medNorthWest;
    [SerializeField] Tile _medSouth;
    [SerializeField] Tile _medSouthEast;
    [SerializeField] Tile _medSouthWest;
    [SerializeField] Tile _medEast;
    [SerializeField] Tile _medWest;

    [Header("Slow Tiles")]
    [SerializeField] Tile _slowNorth;
    [SerializeField] Tile _slowNorthEast;
    [SerializeField] Tile _slowNorthWest;
    [SerializeField] Tile _slowSouth;
    [SerializeField] Tile _slowSouthEast;
    [SerializeField] Tile _slowSouthWest;
    [SerializeField] Tile _slowEast;
    [SerializeField] Tile _slowWest;

    [SerializeField] List<Tile> _slopeTiles = new List<Tile>();

    [Serializable]
    public struct TileSlopeAndSpeedMapping
    {
        public Tile TileType;
        public Vector2 SlopeDirection;
        public float SlopeSpeedModifier;
    }
    [SerializeField] public List<TileSlopeAndSpeedMapping> TileToSlopeMap = new List<TileSlopeAndSpeedMapping>();
    [SerializeField] List<Vector3Int> _tilePositionsWithSlopes = new List<Vector3Int>();
    /*
    [Serializable]
    public struct TileSlopeDirectionAndSpeed
    {
        public Vector3Int TilePosition;
        public Vector2 SlopeDirection;
        public float SlopeSpeedModifier;
    }

    [SerializeField] public TileSlopeDirectionAndSpeed[] TilesWithSlope;
    [SerializeField] List<Vector3Int> _tilePositionsWithSlopes = new List<Vector3Int>();*/

    private void Awake()
    {
        MakeInstance();
        // Get a mapping of tile type to slope/slope speed
        MapTiles();
        // make sure the direction tile tilemap is accessible?
        if (!_directionTiles)
            _directionTiles = GameObject.FindGameObjectWithTag("DirectionTiles").GetComponent<Tilemap>();
        // Find the positions of all tiles with a slope
        GetTilesWithSlopes();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetDirectiontile(Vector3Int tilePos, Vector2 slopeDir, float slopeSpeed)
    {
        if (slopeDir == Vector2.zero || slopeSpeed == 0f)
            return;

        if (_directionTiles.GetTile(tilePos) == null)
        {
            Tile tileToSet = null;
            if (slopeDir.y > 0) // northward
            {
                if (slopeDir.x > 0) // north east
                {
                    if (slopeSpeed == _slowSpeed)
                        tileToSet = _slowNorthEast;
                    else if (slopeSpeed == _medSpeed)
                        tileToSet = _medNorthEast;
                    else if (slopeSpeed == _fastSpeed)
                        tileToSet = _fastNorthEast;
                }
                else if (slopeDir.x < 0) // north west
                {
                    if (slopeSpeed == _slowSpeed)
                        tileToSet = _slowNorthWest;
                    else if (slopeSpeed == _medSpeed)
                        tileToSet = _medNorthWest;
                    else if (slopeSpeed == _fastSpeed)
                        tileToSet = _fastNorthWest;
                }
                else // true north
                {
                    if (slopeSpeed == _slowSpeed)
                        tileToSet = _slowNorth;
                    else if (slopeSpeed == _medSpeed)
                        tileToSet = _medNorth;
                    else if (slopeSpeed == _fastSpeed)
                        tileToSet = _fastNorth;
                }
            }
            else if (slopeDir.y < 0) // southward
            {
                if (slopeDir.x > 0) // South east
                {
                    if (slopeSpeed == _slowSpeed)
                        tileToSet = _slowSouthEast;
                    else if (slopeSpeed == _medSpeed)
                        tileToSet = _medSouthEast;
                    else if (slopeSpeed == _fastSpeed)
                        tileToSet = _fastSouthEast;
                }
                else if (slopeDir.x < 0) // South west
                {
                    if (slopeSpeed == _slowSpeed)
                        tileToSet = _slowSouthWest;
                    else if (slopeSpeed == _medSpeed)
                        tileToSet = _medSouthWest;
                    else if (slopeSpeed == _fastSpeed)
                        tileToSet = _fastSouthWest;
                }
                else // true South
                {
                    if (slopeSpeed == _slowSpeed)
                        tileToSet = _slowSouth;
                    else if (slopeSpeed == _medSpeed)
                        tileToSet = _medSouth;
                    else if (slopeSpeed == _fastSpeed)
                        tileToSet = _fastSouth;
                }
            }
            else // neutral aka only east/west, no north/south direction
            {
                if (slopeDir.x > 0) // eastward
                {
                    if (slopeSpeed == _slowSpeed)
                        tileToSet = _slowEast;
                    else if (slopeSpeed == _medSpeed)
                        tileToSet = _medEast;
                    else if (slopeSpeed == _fastSpeed)
                        tileToSet = _fastEast;
                }
                else // westward
                {
                    if (slopeSpeed == _slowSpeed)
                        tileToSet = _slowWest;
                    else if (slopeSpeed == _medSpeed)
                        tileToSet = _medWest;
                    else if (slopeSpeed == _fastSpeed)
                        tileToSet = _fastWest;
                }
            }

            if (tileToSet != null)
                _directionTiles.SetTile(tilePos, tileToSet);
        }
    }
    void MapTiles()
    {
        foreach (Tile tile in _slopeTiles)
        {
            if (TileToSlopeMap.Any(x => x.TileType == tile))
                continue;
            TileSlopeAndSpeedMapping tileSlopeAndSpeedMapping = new TileSlopeAndSpeedMapping();
            tileSlopeAndSpeedMapping.TileType = tile;
            tileSlopeAndSpeedMapping.SlopeDirection = GetTileDirectionFromName(tile.name);
            tileSlopeAndSpeedMapping.SlopeSpeedModifier = GetTileSpeedFromName(tile.name);
            TileToSlopeMap.Add(tileSlopeAndSpeedMapping);
        }
    }
    Vector2 GetTileDirectionFromName(string tileName)
    {
        Vector2 tileDir = Vector2.zero;

        if (tileName.ToLower().Contains("north"))
        {
            if (tileName.ToLower().Contains("east"))
            {
                tileDir = new Vector2(1f, 1f);
            }
            else if (tileName.ToLower().Contains("west"))
            {
                tileDir = new Vector2(-1f, 1f);
            }
            else
            {
                tileDir = new Vector2(0f, 1f);
            }
        }
        else if (tileName.ToLower().Contains("south"))
        {
            if (tileName.ToLower().Contains("east"))
            {
                tileDir = new Vector2(1f, -1f);
            }
            else if (tileName.ToLower().Contains("west"))
            {
                tileDir = new Vector2(-1f, -1f);
            }
            else
            {
                tileDir = new Vector2(0f, -1f);
            }
        }
        else if (tileName.ToLower().Contains("east"))
        {
            tileDir = new Vector2(1f, 0f);
        }
        else if (tileName.ToLower().Contains("west"))
        {
            tileDir = new Vector2(-1f, 0f);
        }

        return tileDir.normalized;
    }
    float GetTileSpeedFromName(string tileName)
    {
        float speed = 0f;

        if (tileName.ToLower().Contains("slow"))
        {
            speed = _slowSpeed;
        }
        else if (tileName.ToLower().Contains("medium"))
        {
            speed = _medSpeed;
        }
        else if (tileName.ToLower().Contains("fast"))
        {
            speed = _fastSpeed;
        }
        return speed;
    }
    void GetTilesWithSlopes()
    {
        foreach (var pos in _directionTiles.cellBounds.allPositionsWithin)
        {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            if (_directionTiles.HasTile(localPlace))
            {
                _tilePositionsWithSlopes.Add(localPlace);
            }
        }
    }
    public Tuple<Vector2, float> GetSlopeDirection(Vector3Int tilePos)
    {
        // Get the cell position that the ball is on
        //Vector3Int tileBallIsOn = MyTiles.WorldToCell(ballPos);

        // Check to see if that tile has a slope
        if (_tilePositionsWithSlopes.Contains(tilePos))
        {
            //TileSlopeDirectionAndSpeed slopeToReturn = TilesWithSlope.FirstOrDefault(x => x.TilePosition == tilePos);
            //return new Tuple<Vector2, float>(slopeToReturn.SlopeDirection, slopeToReturn.SlopeSpeedModifier);

            TileBase tile = _directionTiles.GetTile(tilePos);
            TileSlopeAndSpeedMapping tileToSlopeMap = TileToSlopeMap.FirstOrDefault(x => x.TileType == tile);

            return new Tuple<Vector2, float>(tileToSlopeMap.SlopeDirection, tileToSlopeMap.SlopeSpeedModifier);

        }
        else // If the tile position isn't in the list of tiles with slopes, return a direction of 0 and a speed modifier of 0f
        {
            return new Tuple<Vector2, float>(Vector2.zero, 0f);
        }
    }
}
