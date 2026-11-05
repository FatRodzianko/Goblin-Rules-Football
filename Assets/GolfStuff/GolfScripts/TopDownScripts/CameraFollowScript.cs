using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

// Runs before CinemachineBrain's LateUpdate (default order 0), so the brain always reads this frame's follow
// position; at equal order the two run in undefined order, and frames where the brain went first rendered last
// frame's position - an on-and-off one-frame lag that showed as screen jitter on whatever the camera tracked.
[DefaultExecutionOrder(-100)]
public class CameraFollowScript : MonoBehaviour
{
    [SerializeField] public GameObject followTarget;
    [SerializeField] public Transform FollowTargetTransform;
    [SerializeField] public CinemachineVirtualCamera MyCinemachineVirtualCamera;
    // Start is called before the first frame update
    void Start()
    {
        if (!MyCinemachineVirtualCamera)
            MyCinemachineVirtualCamera = this.GetComponent<CinemachineVirtualCamera>();
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
    public void UpdateCameraFollow(Transform newFollowTarget)
    {
        FollowTargetTransform = newFollowTarget;
        MyCinemachineVirtualCamera.Follow = newFollowTarget;
    }
    public void StopFollowingTarget()
    {
        MyCinemachineVirtualCamera.Follow = null;
    }
    public void FollowTargetAgain()
    {
        MyCinemachineVirtualCamera.Follow = FollowTargetTransform;
    }
}
