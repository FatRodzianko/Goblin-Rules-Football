using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailParticleScript : MonoBehaviour
{
    [SerializeField] public float lifeTime = 0.25f;
    public float timeAlive = 0f;
    public float speed;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (lifeTime > timeAlive)
            timeAlive += Time.deltaTime;
        else
            Destroy(this.gameObject);
    }
    public void SetSpeed(float particleSpeed)
    {
        speed = particleSpeed;
        SetParticleSize();
        SetParticleColor();
    }
    void SetParticleSize()
    {
        if (speed < 5f)
            this.transform.localScale = new Vector3(1, 1, 1);
        else if (speed < 10f)
            this.transform.localScale = new Vector3(2, 2, 2);
        else if (speed < 30f)
            this.transform.localScale = new Vector3(3, 3, 3);
        else if (speed < 55f)
            this.transform.localScale = new Vector3(4, 4, 4);
    }
    void SetParticleColor()
    { 

    }

}
