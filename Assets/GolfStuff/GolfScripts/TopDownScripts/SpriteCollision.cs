using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteCollision : MonoBehaviour
{
    [SerializeField] public SpriteRenderer MySpriteRenderer;
    [SerializeField] int _defaultOrderInLayer;
    [SerializeField] Collider2D _myCollider;
    [SerializeField] public SpriteMask MySpriteMask;

    [Header("Sprite Colors")]
    [SerializeField] Color _noTransparency = new Color(1f, 1f, 1f, 1f);
    [SerializeField] Color _transparent = new Color(1f, 1f, 1f, 0.5f);

    [Header("Transparency routine stuff?")]
    private bool _fadeInTransparencyRoutineRunning = false;
    private bool _fadeOutTransparencyRoutineRunning = false;


    // Start is called before the first frame update
    void Start()
    {
        _defaultOrderInLayer = MySpriteRenderer.sortingOrder;
        if (!MySpriteMask)
            MySpriteMask = this.GetComponent<SpriteMask>();
        MySpriteMask.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "GolfBallSprite")
        {
            if (collision.transform.parent.position.y > this.transform.position.y)
            {
                //this.MySpriteRenderer.color = _transparent;
                StartCoroutine(FadeInTransparency());
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (MySpriteRenderer.sortingOrder != _defaultOrderInLayer)
            MySpriteRenderer.sortingOrder = _defaultOrderInLayer;

        if (collision.tag == "GolfBallSprite")
        {
            //this.MySpriteRenderer.color = _noTransparency;
            StartCoroutine(FadeOutTransparency());
        }

        // Find any objects overlapping this renderer. If they are below it, reset their order in layer as well? Only if they aren't set to the default alrady? Would need to be "recursive" for each object?
    }
    IEnumerator FadeInTransparency()
    {
        _fadeOutTransparencyRoutineRunning = false;
        _fadeInTransparencyRoutineRunning = true;
        while (_fadeInTransparencyRoutineRunning)
        {
            Color currentColor = this.MySpriteRenderer.color;
            currentColor.a -= 0.1f;

            if (currentColor.a <= _transparent.a)
            {
                this.MySpriteRenderer.color = _transparent;
                _fadeInTransparencyRoutineRunning = false;
            }
                
            this.MySpriteRenderer.color = currentColor;
            yield return new WaitForSeconds(0.1f);
        }
        yield break;


    }
    IEnumerator FadeOutTransparency()
    {
        _fadeInTransparencyRoutineRunning = false;
        _fadeOutTransparencyRoutineRunning = true;
        while (_fadeOutTransparencyRoutineRunning)
        {
            Color currentColor = this.MySpriteRenderer.color;
            currentColor.a += 0.1f;

            if (currentColor.a >= _noTransparency.a)
            {
                this.MySpriteRenderer.color = _noTransparency;
                _fadeOutTransparencyRoutineRunning = false;
            }
            this.MySpriteRenderer.color = currentColor;
            yield return new WaitForSeconds(0.1f);
        }
        yield break;


    }
    public void UpdateSpriteOrderInLayer(int newOrderInLayer)
    {
        if (MySpriteRenderer.sortingOrder != newOrderInLayer)
            MySpriteRenderer.sortingOrder = newOrderInLayer;

        // Find any objects overlapping this renderer. If they are below it, increase their order in layer as well? Would need to be "recursive" for each object?
    }
    public void CheckForOverlappingSpriteColliders()
    {
        // use Collider2d.cast from _myCollider to get all colliders that overlap this collider. Use that to check if they need to have their sorting order raised/lowered
        // https://docs.unity3d.com/ScriptReference/Collider2D.Cast.html
    }
}
