using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    private void Awake()
    {
        MakeInstance();
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
        if (!_directionTiles)
            _directionTiles = GameObject.FindGameObjectWithTag("DirectionTiles").GetComponent<Tilemap>();
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
}
