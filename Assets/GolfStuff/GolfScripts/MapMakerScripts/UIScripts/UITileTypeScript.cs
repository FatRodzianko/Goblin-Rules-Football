using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITileTypeScript : MonoBehaviour
{
    [SerializeField] public Transform ItemHolder;
    [SerializeField] UITileTypes _uiTileType;
    private void Awake()
    {
        if (!ItemHolder)
        {
            ItemHolder = this.transform.Find("Items");
        }
    }
    public UITileTypes UITileType
    {
        get
        {
            return _uiTileType;
        }
        set
        {
            if (value != null)
            {
                _uiTileType = value;
            }
        }
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
