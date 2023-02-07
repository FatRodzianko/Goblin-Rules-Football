using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpriteCollision : MonoBehaviour
{
    [SerializeField] GameObject _shadowObject;
    [SerializeField] CircleCollider2D _circleCollider2D;
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] int _defaultOrderInLayer;
    [SerializeField] GolfBallTopDown _golfBallTopDown;
    [SerializeField] TrailRenderer _trailRenderer;
    // Start is called before the first frame update
    void Start()
    {
        _defaultOrderInLayer = _spriteRenderer.sortingOrder;
        if (!_golfBallTopDown)
            _golfBallTopDown = transform.parent.GetComponent<GolfBallTopDown>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdateColliderRadius(float radius)
    {
        _circleCollider2D.radius = radius;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        try
        {
            IncreaseSortingLayer(collision);
            /*SpriteRenderer collisionRenderer = collision.GetComponent<SpriteRenderer>();
            if (collisionRenderer.sortingLayerID == _spriteRenderer.sortingLayerID)
            {
                if (collision.transform.position.y > _shadowObject.transform.position.y)
                {
                    int collisionOrderInLayer = collisionRenderer.sortingOrder;
                    if (_spriteRenderer.sortingOrder <= collisionOrderInLayer)
                    {
                        _spriteRenderer.sortingOrder = collisionOrderInLayer + 1;
                    }
                }
                IncreaseSortingLayer(collisionRenderer, collision.transform);
            }*/
        }
        catch (Exception e)
        {
            Debug.Log("BallSpriteCollision: OnTriggerEnter2D: could not get sprite renderer from collision object");
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        try
        {
            IncreaseSortingLayer(collision);
            /*SpriteRenderer collisionRenderer = collision.GetComponent<SpriteRenderer>();
            if (collisionRenderer.sortingLayerID == _spriteRenderer.sortingLayerID)
            {
                if (collision.transform.position.y > _shadowObject.transform.position.y)
                {
                    int collisionOrderInLayer = collisionRenderer.sortingOrder;
                    if (_spriteRenderer.sortingOrder <= collisionOrderInLayer)
                    {
                        _spriteRenderer.sortingOrder = collisionOrderInLayer + 1;
                    }
                }
                IncreaseSortingLayer(collisionRenderer, collision.transform);
            }*/
        }
        catch (Exception e)
        {
            Debug.Log("BallSpriteCollision: OnTriggerStay2D: could not get sprite renderer from collision object");
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        try
        {
            SpriteCollision spriteCollision = collision.GetComponent<SpriteCollision>();
            if (spriteCollision.MySpriteMask.enabled && collision.transform.position.y <= _shadowObject.transform.position.y && spriteCollision.MySpriteRenderer.sortingOrder <= _spriteRenderer.sortingOrder)
            {
                spriteCollision.MySpriteMask.enabled = false;
                Debug.Log("BallSpriteCollision: Setting sprite mask to FALSE for: " + collision.gameObject.name);
            }
            if (_spriteRenderer.sortingOrder != _defaultOrderInLayer)
            {
                _spriteRenderer.sortingOrder = _defaultOrderInLayer;
                _trailRenderer.sortingOrder = _spriteRenderer.sortingOrder - 1;
            }
                
        }
        catch (Exception e)
        {
            Debug.Log("BallSpriteCollision: OnTriggerExit2D: could not reset sorting order for sprite. Error: " + e);
        }
    }
    void IncreaseSortingLayer(Collider2D collision)
    {
        if (_golfBallTopDown.isRolling)
            return;
        SpriteCollision spriteCollision = collision.GetComponent<SpriteCollision>();
        SpriteRenderer collisionRenderer = spriteCollision.MySpriteRenderer;
        if (collisionRenderer.sortingLayerID == _spriteRenderer.sortingLayerID)
        {
            if (collision.transform.position.y > _shadowObject.transform.position.y)
            {   
                int collisionOrderInLayer = collisionRenderer.sortingOrder;
                if (_spriteRenderer.sortingOrder <= collisionOrderInLayer)
                {
                    _spriteRenderer.sortingOrder = collisionOrderInLayer + 2;
                    //_trailRenderer.sortingOrder = _spriteRenderer.sortingOrder -1;
                }
                //Debug.Log("BallSpriteCollision: IncreaseSortingLayer: Increasing BALL SPRITE sorting order to: " + _spriteRenderer.sortingOrder.ToString()); 
            }
            else if (collision.transform.position.y <= _shadowObject.transform.position.y  && collisionRenderer.sortingOrder <= _spriteRenderer.sortingOrder && !spriteCollision.MySpriteMask.enabled)
            {
                /*int newOrder = _spriteRenderer.sortingOrder + 1;
                Debug.Log("BallSpriteCollision: IncreaseSortingLayer: Increasing OTHER SPRITE sorting order to: " + newOrder);
                //SpriteCollision spriteCollision = collision.GetComponent<SpriteCollision>();
                spriteCollision.UpdateSpriteOrderInLayer(_spriteRenderer.sortingOrder + 1);*/
                //Debug.Log("BallSpriteCollision: IncreaseSortingLayer: Ball Sprite should be BEHIND sprite collision. Setting sort order to sprite collision sort order of: " + collisionRenderer.sortingOrder.ToString());
                //_spriteRenderer.sortingOrder = collisionRenderer.sortingOrder;
                //Debug.Log("BallSpriteCollision: IncreaseSortingLayer: Ball Sprite should be BEHIND sprite collision. Setting sprite mask to true for: " + collision.gameObject.name);
                spriteCollision.MySpriteMask.enabled = true;

            }
        }
    }
}
