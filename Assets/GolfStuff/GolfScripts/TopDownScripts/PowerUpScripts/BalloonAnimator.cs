using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonAnimator : MonoBehaviour
{
    [SerializeField] BalloonPowerUp _myBallon;

    [Header("Animation Stuff")]
    [SerializeField] Animator _animator;
    //[SerializeField] NetworkAnimator _networkAnimator;

    [Header("Animations")]
    [SerializeField] string _heightOfBallon;
    [SerializeField] string _popAnimation;
    [SerializeField] string _idleAnimation;

    [Header("Animation Stats")]
    [SerializeField] bool _isIdle = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_isIdle)
            _animator.Play(_idleAnimation);
    }
    public void SetHeightOfBallon(string height)
    {
        _heightOfBallon = height;
        _popAnimation = _heightOfBallon + _popAnimation;
        _idleAnimation = _heightOfBallon + _idleAnimation;
        Debug.Log("SetHeightOfBallon: " + _heightOfBallon + " : " + _popAnimation + " : " + _idleAnimation);
    }
    public void SetIsIdle(bool isIdle)
    {
        Debug.Log("SetIsIdle");
        _isIdle = isIdle;
    }
}
