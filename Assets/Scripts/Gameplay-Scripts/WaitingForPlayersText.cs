using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaitingForPlayersText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textObject;
    public int dotCount = 3;
    // Start is called before the first frame update
    void Start()
    {
        //DontDestroyOnLoad(this.transform.parent);
        InvokeRepeating("UpdateDotsInText", 1.0f, 0.33f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void UpdateDotsInText()
    {
        textObject.text = "WAITING FOR PLAYERS";
        if (dotCount == 0)
        {
            textObject.text += ".";
            dotCount = 1;
            return;
        }
        else if (dotCount == 1)
        {
            textObject.text += "..";
            dotCount = 2;
            return;
        }
        else if (dotCount == 2)
        {
            textObject.text += "...";
            dotCount = 3;
            return;
        }
        else if (dotCount == 3)
        {
            //textObject.text += "";
            dotCount = 0;
            return;
        }
    }
}
