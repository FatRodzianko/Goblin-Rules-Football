using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfBall : MonoBehaviour
{
    [SerializeField] public Rigidbody2D rb;
    [SerializeField] Vector3 resetPosition;
    [SerializeField] Quaternion resetRotation;
    public Vector2 angle;
    public float power;
    public float spin;

    public float angularDragAir = 0.05f;
    public float angularDragFairway = 5.0f;
    public float angularDragGreen = 1.0f;
    public float angularDragRough = 50.0f;

    public bool isHit = false;


    // Start is called before the first frame update
    void Start()
    {
        resetPosition = this.transform.position;
        resetRotation = this.transform.rotation;
        ResetPosition(true);
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            HitBall(angle, power, spin);
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            ResetPosition(true);
        }*/
        if (isHit && Mathf.Abs(rb.velocity.x) < 0.01f && Mathf.Abs(rb.velocity.y) < 0.01f)
        {
            ResetPosition(false);
            Debug.Log("Ball stopped moving after hit");
        }
    }
    public void HitBall(Vector3 hitAngle, float hitPower, float hitSpin)
    {
        Debug.Log("Hitball");
        //rb.AddForce(hitAngle.normalized * hitPower, ForceMode2D.Impulse);
        rb.AddForce(hitAngle.normalized * hitPower, ForceMode2D.Force);
        rb.AddTorque(hitSpin);
        //isHit = true;
        StartCoroutine(WasHitDelay());
    }
    public void ResetPosition(bool positionReset)
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        if (positionReset)
        {
            this.transform.position = resetPosition;
            this.transform.rotation = resetRotation;
        }
        rb.angularDrag = angularDragAir;
        isHit = false;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "groundRough" && isHit)
        {
            Debug.Log("OnCollisionEnter2D: Changing angulardrag to Rough");
            rb.angularDrag = angularDragRough;
        }
        if (collision.gameObject.tag == "groundFairway" && isHit)
        {
            Debug.Log("OnCollisionEnter2D: Changing angulardrag to Fairway");
            rb.angularDrag = angularDragFairway;
        }
        if (collision.gameObject.tag == "groundGreen" && isHit)
        {
            Debug.Log("OnCollisionEnter2D: Changing angulardrag to Green");
            rb.angularDrag = angularDragGreen ;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "groundRough")
        {
            Debug.Log("OnCollisionExit2D: Changing angulardrag to Air");
            rb.angularDrag = angularDragAir;
        }
        if (collision.gameObject.tag == "groundFairway")
        {
            Debug.Log("OnCollisionExit2D: Changing angulardrag to Air");
            rb.angularDrag = angularDragAir;
        }
        if (collision.gameObject.tag == "groundGreen")
        {
            Debug.Log("OnCollisionExit2D: Changing angulardrag to Air");
            rb.angularDrag = angularDragAir;
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "groundRough" && isHit)
        {
            Debug.Log("OnCollisionEnter2D: Changing angulardrag to Rough");
            rb.angularDrag = angularDragRough;
        }
        if (collision.gameObject.tag == "groundFairway" && isHit)
        {
            Debug.Log("OnCollisionEnter2D: Changing angulardrag to Fairway");
            rb.angularDrag = angularDragFairway;
        }
        if (collision.gameObject.tag == "groundGreen" && isHit)
        {
            Debug.Log("OnCollisionEnter2D: Changing angulardrag to Green");
            rb.angularDrag = angularDragGreen;
        }
    }
    IEnumerator WasHitDelay()
    {
        yield return new WaitForSeconds(0.1f);
        isHit = true;
    }
}
