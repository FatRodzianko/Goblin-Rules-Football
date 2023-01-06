using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteCollision : MonoBehaviour
{
    [SerializeField] public SpriteRenderer MySpriteRenderer;
    [SerializeField] int _defaultOrderInLayer;
    [SerializeField] Collider2D _myCollider;
    [SerializeField] public SpriteMask MySpriteMask;
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
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (MySpriteRenderer.sortingOrder != _defaultOrderInLayer)
            MySpriteRenderer.sortingOrder = _defaultOrderInLayer;

        // Find any objects overlapping this renderer. If they are below it, reset their order in layer as well? Only if they aren't set to the default alrady? Would need to be "recursive" for each object?
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
