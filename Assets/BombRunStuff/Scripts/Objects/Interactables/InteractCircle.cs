using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractCircle : BaseBombRunObstacle, IInteractable
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    private bool _isGreen;

    private Action _onInteractionComplete;
    private bool _isActive;
    private float _timer;

    private void Awake()
    {
        if (!_spriteRenderer)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
    private void Start()
    {
        SetColorGreen();
    }
    private void Update()
    {
        if (!_isActive)
        {
            return;
        }

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _isActive = false;
            _onInteractionComplete();
        }
    }
    private void SetColorGreen()
    {
        _spriteRenderer.color = Color.green;
        _isGreen = true;
    }
    private void SetColorRed()
    {
        _spriteRenderer.color = Color.red;
        _isGreen = false;
    }

    public void Interact(Action onInteractionComplete)
    {
        this._onInteractionComplete = onInteractionComplete;
        _isActive = true;
        _timer = 0.5f;

        if (_isGreen)
        {
            SetColorRed();
        }
        else
        {
            SetColorGreen();
        }
    }
}
