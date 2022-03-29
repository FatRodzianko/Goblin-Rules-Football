using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PossessionBar : MonoBehaviour
{
    [SerializeField] RectTransform myTransform;
    [SerializeField] Image myImage;
    [SerializeField] Color tier1Color;
    [SerializeField] Color tier2Color;
    [SerializeField] Color tier3Color;
    [SerializeField] Color tier4Color;
    [SerializeField] Color tier5Color;
    // Start is called before the first frame update
    void Start()
    {
        //myTransform = this.GetComponent<RectTransform>();
        //this.transform.localScale = new Vector3(0f, 1f, 1f);
        myTransform.localScale = new Vector3(0f, 1f, 1f);
        myImage.color = tier1Color;
        //myImage = this.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdatePossessionBar(float possession)
    {
        myTransform.localScale = new Vector3((possession / 100), 1f, 1f);
        if (possession < 30f)
        {
            myImage.color = tier1Color;
        }
        if (possession >= 30f && possession < 50f)
        {
            myImage.color = tier2Color;
        }
        if (possession >= 50f && possession < 70f)
        {
            myImage.color = tier3Color;
        }
        if (possession >= 70f && possession < 90f)
        {
            myImage.color = tier4Color;
        }
        if (possession >= 90f)
        {
            myImage.color = tier5Color;
        }
    }
}
