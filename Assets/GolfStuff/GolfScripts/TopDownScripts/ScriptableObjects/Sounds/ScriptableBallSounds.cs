using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New_Scriptable_Sound", menuName = "Sound Scriptable")]
public class ScriptableBallSounds : ScriptableObject
{
    public string HitTeeOff;
    public string HitOffGround;
    public string HitShank;
    public string BounceGreen;
    public string BounceFairway;
    public string BounceRough;
    public string BounceSand;
    public string BounceWater;
    public string BallInHole;
}
