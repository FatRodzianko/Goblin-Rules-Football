using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TouchDownTextGradient : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI touchDownText;
    [SerializeField] Color color1;
    [SerializeField] Color color2;
    [SerializeField] Color color3;
    [SerializeField] Color color4;
    public bool isColorChangeRunning = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetGreenOrGreyColor(bool isGrey)
    {
        if (isGrey)
        {
            color1 = new Color(0.2f, 0.2f, 0.2f, 1.0f);
            color2 = new Color(0.4f, 0.4f, 0.4f, 1.0f);
            color3 = new Color(0.3f, 0.3f, 0.3f, 1.0f);
            color4 = new Color(0f, 0f, 0f, 1.0f);
        }
        else
        {
            color1 = new Color(0f, 1.0f, 0f, 1.0f);
            color2 = new Color(0f, 0.6f, 0f, 1.0f);
            color3 = new Color(0f, 0.4f, 0f, 1.0f);
            color4 = new Color(0f, 0.2f, 0f, 1.0f);
        }
        touchDownText.colorGradient = new VertexGradient(color1, color2, color3, color4);
        //ActivateGradient();
    }
    public void ActivateGradient()
    {
        if (!isColorChangeRunning)
        {
            IEnumerator touchDownTextRoutine = GradientRoutine();
            StartCoroutine(touchDownTextRoutine);
        }
        
    }
    public IEnumerator GradientRoutine()
    {
        isColorChangeRunning = true;
        touchDownText.colorGradient = new VertexGradient(color1, color2, color3, color4);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color4, color1, color2, color3);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color3, color4, color1, color2);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color2, color3, color4, color1);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color1, color2, color3, color4);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color4, color1, color2, color3);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color3, color4, color1, color2);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color2, color3, color4, color1);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color1, color2, color3, color4);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color4, color1, color2, color3);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color3, color4, color1, color2);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color2, color3, color4, color1);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color1, color2, color3, color4);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color4, color1, color2, color3);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color3, color4, color1, color2);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color2, color3, color4, color1);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color1, color2, color3, color4);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color4, color1, color2, color3);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color3, color4, color1, color2);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color2, color3, color4, color1);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color1, color2, color3, color4);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color4, color1, color2, color3);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color3, color4, color1, color2);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color2, color3, color4, color1);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color1, color2, color3, color4);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color4, color1, color2, color3);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color3, color4, color1, color2);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color2, color3, color4, color1);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color1, color2, color3, color4);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color4, color1, color2, color3);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color3, color4, color1, color2);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color2, color3, color4, color1);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color1, color2, color3, color4);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color4, color1, color2, color3);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color3, color4, color1, color2);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color2, color3, color4, color1);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color1, color2, color3, color4);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color4, color1, color2, color3);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color3, color4, color1, color2);
        yield return new WaitForSeconds(0.1f);
        touchDownText.colorGradient = new VertexGradient(color2, color3, color4, color1);
        yield return new WaitForSeconds(0.1f);
        isColorChangeRunning = false;
    }
}
