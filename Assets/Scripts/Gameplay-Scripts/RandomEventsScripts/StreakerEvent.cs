using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StreakerEvent : NetworkBehaviour
{
    [Header("Spawn Y Values")]
    [SerializeField] float topY;
    [SerializeField] float bottomY;

    [Header("Spawn X Values")]
    public float spawnX;

    [Header("Movement Direction and Other Stuff")]
    public bool isRunning = false;
    public Vector2 direction = Vector2.zero;
    public Vector3 finalDestination = Vector3.zero;
    public Vector3 spawnPoint = Vector3.zero;
    public float speed;
    [SerializeField] Rigidbody2D rb;

    [Header("Animation Stuff")]
    [SerializeField] Animator myAnimator;
    [SerializeField] SpriteRenderer myRenderer;
    [SerializeField] BoxCollider2D myCollider;
    [SerializeField] string runUp;
    [SerializeField] string runDown;

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
        //this.StartAnimation();
        IEnumerator timetoWait = WaitRandomTime(Random.Range(0.15f, 0.35f));
        StartCoroutine(timetoWait);
    }
    [ServerCallback]
    private void FixedUpdate()
    {
        if (isRunning)
        {
            if (Vector3.Distance(this.transform.position, finalDestination) <= 0.2f)
                NetworkServer.Destroy(this.gameObject);
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isServer)
        {
            Debug.Log("OnTriggerEnter2D for StreakerEvent: " + this.name);
            if (collision.tag == "Goblin")
            {
                Debug.Log("StreakerEvent: collided with goblin named: " + collision.transform.name);
                uint goblinNetId = collision.transform.gameObject.GetComponent<NetworkIdentity>().netId;

                GoblinScript goblinScript = collision.transform.gameObject.GetComponent<GoblinScript>();
                if (goblinScript.canCollide)
                    goblinScript.KnockOutGoblin(false);

            }
        }
    }
    public void StartAnimation()
    {
        Debug.Log("StartAnimation for streaker event");
        // Get where the Goblin has spawned. Top half of the field versus the bottom half
        Vector3 eventPosition = this.transform.position;
        if (eventPosition.y >= -0.6f)
        {
            Debug.Log("Streaker-goblin StartAnimation: Goblin is on top half of field");
            spawnPoint = new Vector3(eventPosition.x, topY, 0f);
            finalDestination = new Vector3(eventPosition.x, bottomY, 0f);
        }
        else
        {
            Debug.Log("Streaker-goblin StartAnimation: Goblin is on bottom half of field");
            spawnPoint = new Vector3(eventPosition.x, bottomY, 0f);
            finalDestination = new Vector3(eventPosition.x, topY, 0f);
        }

        // Move the streaker goblin to the spawnPoint and then turn on the renderer and colliders
        this.gameObject.transform.position = spawnPoint;
        //myRenderer.enabled = true;
        //myCollider.enabled = true;


        if (finalDestination.y > spawnPoint.y)
        {
            direction = new Vector3(0f, 1.0f);
            //myAnimator.Play(runUp);
            myAnimator.SetBool("runUp", true);
        }
        else
        {
            direction = new Vector3(0f, -1.0f);
            //myAnimator.Play(runDown);
            myAnimator.SetBool("runDown", true);
        }
        isRunning = true;
    }
    IEnumerator WaitRandomTime(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        this.speed += Random.Range(-2.0f, 2f);
        this.StartAnimation();
    }
}
