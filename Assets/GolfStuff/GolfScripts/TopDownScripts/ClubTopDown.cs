using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClubTopDown : MonoBehaviour
{
    [Header("Club Attributes")]
    [SerializeField] public string ClubType;
    [SerializeField] public string ClubName;
    [SerializeField] public float MaxHitDistance;
    [SerializeField] public float DefaultLaunchAngle;
    [SerializeField] public float SpinDivider;
    [SerializeField] public float MaxTopSpin;
    [SerializeField] public float MaxBackSpin;
    [SerializeField] public float MaxSideSpin;
    [SerializeField] public Sprite ClubTextSprite;
    [SerializeField] public Sprite ClubImageSprite;

    [Header("Ground Distance Modifiers")]
    [SerializeField] public float RoughTerrainDistModifer = 0.8f;
    [SerializeField] public float DeepRoughTerrainDistModifer = 0.7f;
    [SerializeField] public float TrapTerrainDistModifer = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
