using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Component.Animating;
using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class BrokeGlassThrownBottle : NetworkBehaviour
{
    public bool isRandomEvent;
    public bool isThrown;
    public Vector3 startPoint;
    public Vector3 endPoint;
    float throwCount = 0f;
    [SerializeField] public Animator myAnimator;
    NetworkAnimator networkAnimator;
    public GameObject myParentEvent;

    [Header("SFX Stuff")]
    [SerializeField] public string sfxClipName;

    // Start is called before the first frame update
    void Start()
    {
        // The break animation plays on the server; FishNet's NetworkAnimator only sends state changes made through it.
        networkAnimator = GetComponent<NetworkAnimator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Server(Logging = LoggingType.Off)]
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
                    networkAnimator.Play("bottle-break-ground", 0, float.NegativeInfinity);
                    isThrown = false;
                }
            }
            else
            {
                networkAnimator.Play("bottle-break-ground", 0, float.NegativeInfinity);
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
        if (IsServerInitialized)
            InstanceFinder.ServerManager.Despawn(this.gameObject);
    }
    public void ActivateBrokenGlassArea()
    {
        if (IsServerInitialized && isRandomEvent && myParentEvent)
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
    // Animation event: it fires on every machine that plays the clip, so it skips quietly where it doesn't apply.
    [Client(Logging = LoggingType.Off)]
    public void PlaySFXClipEvent()
    {
        //Debug.Log("PlaySFXClipEvent");
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(this.transform.position);
        if (screenPoint.x < 0 || screenPoint.x > 1)
            return;
        //Debug.Log("PlaySFXClipEvent: didn't return");
        SoundManager.instance.PlaySound(sfxClipName, 0.5f);
    }
}
