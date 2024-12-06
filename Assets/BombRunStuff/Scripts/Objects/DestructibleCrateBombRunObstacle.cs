using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleCrateBombRunObstacle : BaseBombRunObstacle
{
    public override void DamageObstacle(int damageAmount)
    {
        this.DestroyObstacle();
    }
}
