using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BrokenGlassEvent : NetworkBehaviour
{
    [SerializeField] GameObject ThrownBottlePrefab;
    [SerializeField] public SpriteRenderer myRenderer;
    [SerializeField] public BoxCollider2D myCollider;

    int lifeTimeCount = 0;
    IEnumerator lifeTimeCounterRoutine;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        lifeTimeCounterRoutine = LifeTimeCounter();
        StartCoroutine(lifeTimeCounterRoutine);
        ThrowBottlesAtField();
    }
    [ServerCallback]
    IEnumerator LifeTimeCounter()
    {
        while (lifeTimeCount < 5.5)
        {
            yield return new WaitForSeconds(1.0f);
            lifeTimeCount++;
        }
        //PowerUpManager.instance.DestroyPowerUp(this.GetComponent<NetworkIdentity>().netId);
        NetworkServer.Destroy(this.gameObject);
        yield break;
    }
    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("OnTriggerEnter2D for PowerUpThrownObject: " + this.name);
        if (collision.tag == "Goblin")
        {
            Debug.Log("PowerUpThrownObject: collided with goblin named: " + collision.transform.name);
            uint goblinNetId = collision.transform.gameObject.GetComponent<NetworkIdentity>().netId;
            GoblinScript goblinScript = collision.transform.gameObject.GetComponent<GoblinScript>();
            if(goblinScript.canCollide)
                goblinScript.KnockOutGoblin(false);
            //NetworkServer.Destroy(this.gameObject);
            //CmdPlayerPickUpFootball(goblinNetId);
        }
    }
    void ThrowBottlesAtField()
    {
        //Get Y position for bottles
        /*float yPosition = 11f;
        if (this.transform.position.y < -0.5f)
            yPosition = -11f;*/
        float yPosition = 15.5f;
        if (this.transform.position.y < -2f)
            yPosition = -15.5f;
        float xPosition = this.transform.position.x;
        IEnumerator throwBottleRoutine = ThrowBottlesRoutine(yPosition, xPosition);
        StartCoroutine(throwBottleRoutine);
    }
    [ServerCallback]
    IEnumerator ThrowBottlesRoutine(float yPosition, float xPosition)
    {
        /*float negOrPos = Random.Range(0, 2) * 2 - 1;
        Vector3 bottleStartPosition = new Vector3((xPosition + (2.5f * negOrPos)), yPosition, 0f);
        Vector3 endPosition = this.transform.position;
        GameObject thrownBottle = Instantiate(ThrownBottlePrefab, this.transform);
        thrownBottle.transform.position = bottleStartPosition;
        NetworkServer.Spawn(thrownBottle);
        thrownBottle.GetComponent<BrokeGlassThrownBottle>().StartThrow(bottleStartPosition, endPosition);*/
        Vector3 endPosition = this.transform.position;
        CreateThrownBottle(yPosition, xPosition, endPosition);
        float randomTime = Random.Range(0.33f, 0.8f);
        yield return new WaitForSeconds(randomTime);
        randomTime = Random.Range(0.33f, 0.8f);
        CreateThrownBottle(yPosition, xPosition, endPosition);
        yield return new WaitForSeconds(randomTime);
        randomTime = Random.Range(0.33f, 0.8f);
        CreateThrownBottle(yPosition, xPosition, endPosition);
        yield return new WaitForSeconds(randomTime);
        randomTime = Random.Range(0.33f, 0.8f);
        CreateThrownBottle(yPosition, xPosition, endPosition);
        yield return new WaitForSeconds(randomTime);
        randomTime = Random.Range(0.33f, 0.8f);
        CreateThrownBottle(yPosition, xPosition, endPosition);
        yield return new WaitForSeconds(randomTime);
        randomTime = Random.Range(0.33f, 0.8f);
        CreateThrownBottle(yPosition, xPosition, endPosition);
        yield return new WaitForSeconds(randomTime);

    }
    [ServerCallback]
    void CreateThrownBottle(float yPosition, float xPosition, Vector3 endPosition)
    {
        float negOrPos = Random.Range(0, 2) * 2 - 1;
        Vector3 bottleStartPosition = new Vector3((xPosition + (2.5f * negOrPos)), yPosition, 0f);
        //GameObject thrownBottle = Instantiate(ThrownBottlePrefab, this.transform);
        GameObject thrownBottle = Instantiate(ThrownBottlePrefab);
        thrownBottle.transform.position = bottleStartPosition;
        NetworkServer.Spawn(thrownBottle);
        endPosition.x += (Random.Range(0f, 0.8f) * negOrPos);
        negOrPos = Random.Range(0, 2) * 2 - 1;
        endPosition.y += (Random.Range(0f, 0.8f) * negOrPos);
        BrokeGlassThrownBottle thrownBottleScript = thrownBottle.GetComponent<BrokeGlassThrownBottle>();
        thrownBottleScript.myParentEvent = this.gameObject;
        thrownBottleScript.StartThrow(bottleStartPosition, endPosition);
    }
    [ClientRpc]
    public void RpcActivateRenderer()
    {
        if (!myRenderer.enabled)
        {
            myRenderer.enabled = true;
        }
    }
}
