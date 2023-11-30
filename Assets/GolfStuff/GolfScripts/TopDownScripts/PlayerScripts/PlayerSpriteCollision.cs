using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpriteCollision : MonoBehaviour
{
    [SerializeField] BoxCollider2D _myCollider;
    [SerializeField] SpriteRenderer _myRenderer;

    [Header("Collider Sizes and Offsets")]
    [SerializeField] Vector2 _sidewaysSwingColliderOffset;
    [SerializeField] Vector2 _sidewaysSwingColliderSize;
    [SerializeField] Vector2 _upSwingColliderOffset;
    [SerializeField] Vector2 _upSwingColliderSize;
    [SerializeField] Vector2 _downSwingColliderOffset;
    [SerializeField] Vector2 _downSwingColliderSize;

    [Header("Sprite Colors")]
    [SerializeField] Color _noTransparency = new Color(1f, 1f, 1f, 1f);
    [SerializeField] Color _transparent = new Color(1f, 1f, 1f, 0.5f);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetColliderOffsetAndPosition(string golferDirection)
    {
        Debug.Log("SetColliderOffsetAndPosition: " + golferDirection);
        if (golferDirection == "up")
        {
            this._myCollider.offset = _upSwingColliderOffset;
            this._myCollider.size = _upSwingColliderSize;
        }
        else if (golferDirection == "down")
        {
            this._myCollider.offset = _downSwingColliderOffset;
            this._myCollider.size = _downSwingColliderSize;
        }
        else
        {
            this._myCollider.offset = _sidewaysSwingColliderOffset;
            this._myCollider.size = _sidewaysSwingColliderSize;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("PlayerSpriteCollision: OnTriggerEnter2D with: " + collision.name);
        if (collision.tag == "GolfLandingTarget" && this.transform.position.y < collision.transform.position.y)
        {
            _myRenderer.color = _transparent;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("PlayerSpriteCollision: OnTriggerExit2D with: " + collision.name);

        if (_myRenderer.color.a < 1)
        {
            Debug.Log("PlayerSpriteCollision: REMOVING transparency");
            _myRenderer.color = _noTransparency;
        }
            
    }
}
