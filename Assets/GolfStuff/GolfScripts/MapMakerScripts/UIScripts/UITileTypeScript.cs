using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITileTypeScript : MonoBehaviour
{
    [SerializeField] public Transform ItemHolder;
    private void Awake()
    {
        if (!ItemHolder)
        {
            ItemHolder = this.transform.Find("Items");
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
