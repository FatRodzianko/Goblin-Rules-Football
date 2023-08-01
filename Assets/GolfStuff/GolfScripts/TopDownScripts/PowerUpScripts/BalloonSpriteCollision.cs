using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonSpriteCollision : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] BalloonPowerUp _myBalloon;
    [SerializeField] PolygonCollider2D _myCollider;
    [SerializeField] SpriteMask _mySpriteMask;

    [Header("Sprites")]
    [SerializeField] Sprite _lowSprite;
    [SerializeField] Sprite _medSprite;
    [SerializeField] Sprite _highSprite;

    [Header("Offsets")]
    [SerializeField] Vector2 _lowOffset;
    [SerializeField] Vector2 _medOffset;
    [SerializeField] Vector2 _highOffset;

    [Header("Collider Points")]
    [SerializeField] Vector2[] _lowColliderPoints;
    [SerializeField] Vector2[] _medColliderPoints;
    [SerializeField] Vector2[] _highColliderPoints;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetHeightOfBalloon(string height)
    {
        Debug.Log("SetHeightOfBalloon: " + height);
        if (height == "high")
        {
            _mySpriteMask.sprite = _highSprite;
            _myCollider.offset = _highOffset;
            _myCollider.SetPath(0, _highColliderPoints);
        }
        else if (height == "med")
        {
            _mySpriteMask.sprite = _medSprite;
            _myCollider.offset = _medOffset;
            _myCollider.SetPath(0, _medColliderPoints);
        }
        else
        {
            _mySpriteMask.sprite = _lowSprite;
            _myCollider.offset = _lowOffset;
            _myCollider.SetPath(0, _lowColliderPoints);
        }

    }
}
