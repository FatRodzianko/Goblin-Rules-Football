using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialObstacleObject : MonoBehaviour
{
    [SerializeField] Collider2D myCollider;
    [SerializeField] bool isTripObject;
    public bool isSlowObject;

    [Header("SFX Types")]
    public bool isGlue;
    public bool isWater;
    public bool isBrush;

    [Header("SFX Stuff")]
    [SerializeField] public string sfxClipName;

    // Start is called before the first frame update
    void Start()
    {
        myCollider = this.GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void DisableColliderDuringPhase(bool enable)
    {
        myCollider.enabled = enable;
    }
    //
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("OnTriggerEnter2D for ObstacleObject: " + this.name);
        if (collision.tag == "Goblin")
        {
            if (isTripObject)
            {
                Debug.Log("ObstacleObject: collided with goblin named: " + collision.transform.name);

                TutorialGoblinScript goblinScript = collision.transform.gameObject.GetComponent<TutorialGoblinScript>();
                if (goblinScript.canCollide && !goblinScript.isGoblinKnockedOut)
                    goblinScript.KnockOutGoblin(false);
                this.PlaySFXClip();
            }
        }
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log("OnTriggerStay2D for ObstacleObject: " + this.name);
        if (collision.tag == "Goblin")
        {
            if (!isTripObject)
            {
                Debug.Log("ObstacleObject: still colliding with goblin named: " + collision.transform.name);
                collision.transform.gameObject.GetComponent<TutorialGoblinScript>().SlowDownObstacleEffect(true);
                if (isSlowObject)
                {
                    collision.transform.gameObject.GetComponent<TutorialGoblinScript>().onWaterSlowDown = this.isWater;
                    collision.transform.gameObject.GetComponent<TutorialGoblinScript>().onBrushSlowDown = this.isBrush;
                    collision.transform.gameObject.GetComponent<TutorialGoblinScript>().onGlueSlowDown = this.isGlue;
                }

            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("OnTriggerStay2D for ObstacleObject: " + this.name);
        if (collision.tag == "Goblin")
        {
            if (!isTripObject)
            {
                Debug.Log("ObstacleObject: still colliding with goblin named: " + collision.transform.name);
                collision.transform.gameObject.GetComponent<TutorialGoblinScript>().SlowDownObstacleEffect(false);
                if (isSlowObject)
                {
                    collision.transform.gameObject.GetComponent<TutorialGoblinScript>().onWaterSlowDown = false;
                    collision.transform.gameObject.GetComponent<TutorialGoblinScript>().onBrushSlowDown = false;
                    collision.transform.gameObject.GetComponent<TutorialGoblinScript>().onGlueSlowDown = false;
                }
            }
        }
    }
    public bool IsOnScreen()
    {
        bool onscreen = false;
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(this.transform.position);
        if (screenPoint.x < 0 || screenPoint.x > 1)
        {
            onscreen = false;
        }
        else
            onscreen = true;
        return onscreen;
    }
    
    void RpcPlaySFXClip()
    {
        if (this.IsOnScreen() && !string.IsNullOrWhiteSpace(sfxClipName))
        {
            SoundManager.instance.PlaySound(sfxClipName, 0.75f);
        }
    }
    
    void PlaySFXClip()
    {
        if (this.IsOnScreen() && !string.IsNullOrWhiteSpace(sfxClipName))
        {
            SoundManager.instance.PlaySound(sfxClipName, 1.0f);
        }

    }
}
