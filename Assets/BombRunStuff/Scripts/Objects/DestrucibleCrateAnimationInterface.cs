using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestrucibleCrateAnimationInterface : MonoBehaviour
{
    [SerializeField] DestructibleCrateBombRunObstacle _destructibleCrateBombRunObstacle;

    public void ExplosionComplete()
    {
        _destructibleCrateBombRunObstacle.DestroyObstacle();
    }
}
