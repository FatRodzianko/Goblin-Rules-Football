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
    [SerializeField] string _crateHitAnimation;
    [SerializeField] string _idleAnimation;

    [Header("Animation Stats")]
    [SerializeField] bool _isIdle = false;
    [SerializeField] bool _isPopped = false;

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
        _crateHitAnimation = _heightOfBallon + _crateHitAnimation;
        _idleAnimation = _heightOfBallon + _idleAnimation;
        Debug.Log("SetHeightOfBallon: " + _heightOfBallon + " : " + _popAnimation + " : " + _idleAnimation);
    }
    public void SetIsIdle(bool isIdle)
    {
        Debug.Log("SetIsIdle");
        if (isIdle)
        {
            StartCoroutine(RandomStartAnimationDelay(isIdle));
        }
        else
            _isIdle = isIdle;
    }
    public void PopBalloon(bool hitCrate)
    {
        if (_isPopped)
            return;
        _isIdle = false;
        if (hitCrate)
        {
            _animator.Play(_crateHitAnimation);
        }
        else
        {
            _animator.Play(_popAnimation);
        }
        _isPopped = true;
    }
    IEnumerator RandomStartAnimationDelay(bool isIdle)
    {   
        yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.5f));
        _isIdle = isIdle;
    }

}
