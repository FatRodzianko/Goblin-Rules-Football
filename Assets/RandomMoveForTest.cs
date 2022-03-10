using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMoveForTest : MonoBehaviour
{
    float minY = -4.5f;
    float maxY = 6.2f;
    int directionModifier = 1;
    public int speed = 4;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        Vector3 position = transform.position;
        position.y += speed * Time.fixedDeltaTime * directionModifier;
        if (position.y > maxY)
            directionModifier = -1;
        if (position.y < minY)
            directionModifier = 1;
        transform.position = position;

    }
}
