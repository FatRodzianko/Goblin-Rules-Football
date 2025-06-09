using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShootSubAction_SelectionOutlineScript : MonoBehaviour
{
    [Header("Sprite Stuff")]
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private LayerMask _layerMask;

    [SerializeField] private Color _notSelected = new Color(1f,1f,1f,0.2f);
    [SerializeField] private Color _selected = Color.yellow;
    [SerializeField] private Color _disabled = Color.red;

    [Header("Status")]
    [SerializeField] private BodyPart _bodyPart;
    [SerializeField] private bool _isDisabled = true;
    private Action<BodyPart> _onClick;

    private void Start()
    {
        //_sprite.color = _notSelected;
        //if (EventSystem.current.IsPointerOverGameObject())
        //{
        //    _sprite.color = _selected;
        //}
    }
    public void SetOnClickAction(Action<BodyPart> onClickAction)
    {
        _onClick = onClickAction;
    }

    private void Update()
    {      
        if (_isDisabled)
            return;

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        RaycastHit2D[] hits = Physics2D.RaycastAll(MouseWorld.GetPosition(), Vector2.zero, 0f, _layerMask);
        if (hits.Length == 0)
        {
            this._sprite.color = _notSelected;
            return;
        }
            
        bool wasThisObjectHit = false;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.transform == this.transform)
            {
                this._sprite.color = _selected;
                wasThisObjectHit = true;
                break;
            }
        }

        if (!wasThisObjectHit)
        {
            this._sprite.color = _notSelected;
            return;
        }

        if (!InputManagerBombRun.Instance.IsMouseButtonDownThisFrame())
        {
            return;
        }
        ClickedOn();

    }
    public void SetDisabled(BodyPartFrozenState state, bool isTargetFriendly)
    {
        Debug.Log("SetDisabled: " + this.name + " state: " + state.ToString() + " is target a friendly unit? " + isTargetFriendly);

        if (isTargetFriendly)
        {
            if (state != BodyPartFrozenState.NotFrozen)
            {
                this._isDisabled = false;
            }
            else
            {
                this._sprite.color = _disabled;
            }
        }
        else
        {
            if (state != BodyPartFrozenState.FullFrozen)
            {
                this._isDisabled = false;
            }
            else
            {
                this._sprite.color = _disabled;
            }
        }

        

    }
    void ClickedOn()
    {
        Debug.Log("ClickedOn: " + this.name + " on " + this.transform.parent.parent.parent.name);
        if (_onClick == null)
            return;
        _onClick(_bodyPart);
    }

}
