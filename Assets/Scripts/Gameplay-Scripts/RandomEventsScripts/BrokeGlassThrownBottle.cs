using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BrokeGlassThrownBottle : NetworkBehaviour
{
    public bool isRandomEvent;
    public bool isThrown;
    public Vector3 startPoint;
    public Vector3 endPoint;
    float throwCount = 0f;
    [SerializeField] public Animator myAnimator;
    public GameObject myParentEvent;

    [Header("SFX Stuff")]
    [SerializeField] public string sfxClipName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [ServerCallback]
    private void FixedUpdate()
    {
        if (isThrown)
        {
            if (throwCount < 1.0f)
            {
                throwCount += 3f * Time.fixedDeltaTime;
                Vector3 newPosition = Vector3.Lerp(startPoint, endPoint, throwCount);
                this.transform.position = newPosition;
                if (throwCount >= 1.0f)
                {
                    myAnimator.Play("bottle-break-ground");
                    isThrown = false;
                }
            }
            else
            {
                myAnimator.Play("bottle-break-ground");
                isThrown = false;
            }
        }
        
    }
    public void StartThrow(Vector3 startP, Vector3 endP)
    {
        startPoint = startP;
        endPoint = endP;
        isThrown = true;
    }
    public void DestroyObject()
    {
        if (isServer)
            NetworkServer.Destroy(this.gameObject);
    }
    public void ActivateBrokenGlassArea()
    {
        if (isServer && isRandomEvent && myParentEvent)
        {
            //BrokenGlassEvent myParent = this.transform.parent.GetComponent<BrokenGlassEvent>();
            BrokenGlassEvent myParent = myParentEvent.GetComponent<BrokenGlassEvent>();
            if (!myParent.myRenderer.enabled)
            {
                myParent.myRenderer.enabled = true;
                myParent.RpcActivateRenderer();
            } 
            if (!myParent.myCollider.enabled)
                myParent.myCollider.enabled = true;
        }
    }
    [ClientCallback]
    public void PlaySFXClip()
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(this.transform.position);
        if (screenPoint.x < 0 || screenPoint.x > 1)
            return;
        SoundManager.instance.PlaySound(sfxClipName, 0.3f);
    }
}
