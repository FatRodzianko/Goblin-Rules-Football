using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QEMarkerScript : MonoBehaviour
{
    public bool isQ;
    public bool canPassToThisGoblin = true;
    CameraMarker myCameraMarker;

    Vector3 newPosition = Vector3.zero;
    Vector3 savedLocalPosition;
    Vector3 myTransformPosition;

    [SerializeField] SpriteRenderer myRenderer;
    [SerializeField] Sprite canPass;
    [SerializeField] Sprite cannotPass;

    // Start is called before the first frame update
    void Start()
    {
        savedLocalPosition = this.transform.localPosition;
        myCameraMarker = Camera.main.GetComponent<CameraMarker>();
    }

    // Update is called once per frame
    void Update()
    {
        myTransformPosition = this.transform.position;
        if (myTransformPosition.y >= 8.5f)
        {
            newPosition = myTransformPosition;
            newPosition.y = 8.5f;
            this.transform.position = newPosition;
        }
        else
        {
            this.transform.localPosition = savedLocalPosition;
        }


        Vector3 screenPoint = Camera.main.WorldToViewportPoint(myTransformPosition);
        if (screenPoint.x < 0)
        {
            myCameraMarker.ActivateGoblinMarker(true, isQ, this.transform.position.y, canPassToThisGoblin);
        }
        else if (screenPoint.x > 1)
        {
            myCameraMarker.ActivateGoblinMarker(false, isQ, this.transform.position.y, canPassToThisGoblin);
        }
        else
        {
            myCameraMarker.DeactivateGoblinMarker(isQ);
        }

        
    }
    private void FixedUpdate()
    {
        
    }
    public void UpdateSpriteForPassing(bool canReceivePass)
    {
        if (canReceivePass)
        {
            myRenderer.sprite = canPass;
            canPassToThisGoblin = true;
        }
        else
        {
            myRenderer.sprite = cannotPass;
            canPassToThisGoblin = false;
        }
            
    }
}
