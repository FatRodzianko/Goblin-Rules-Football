using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowScript : MonoBehaviour
{
    [SerializeField] public GameObject followTarget;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void LateUpdate()
    {
        if (!followTarget)
            return;
        Vector3 followTargetPostion = followTarget.transform.position;
        Vector3 newPos = this.transform.position;
        newPos.x = followTargetPostion.x;
        newPos.y = followTargetPostion.y;
        this.transform.position = newPos;
    }
}
