using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyModInventoryUIManager : MonoBehaviour
{
    [SerializeField] GameObject _bodyModInventoryUIHolder;
    [SerializeField] bool _menuOpen;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (_menuOpen)
            {
                _bodyModInventoryUIHolder.SetActive(false);
                _menuOpen = false;
            }
            else
            {
                _bodyModInventoryUIHolder.SetActive(true);
                _menuOpen = true;
            }
            
        }
    }
}
