using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Component.Animating;
using FishNet;

public class StatueAnimator : MonoBehaviour
{
    [SerializeField] Statue _myStatue;

    [Header("Animation Stuff")]
    [SerializeField] Animator _animator;
    //[SerializeField] NetworkAnimator _networkAnimator;

    [Header("Animations")]
    [SerializeField] string _breakAnimation;
    [SerializeField] string _idleAnimation;
    [SerializeField] string _crackedAnimation;

    // Start is called before the first frame update
    void Start()
    {
        if (!_myStatue)
            _myStatue = this.transform.GetComponent<Statue>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_myStatue.IsBroken && !_myStatue.IsCracked)
        {
            _animator.Play(_idleAnimation);
        }
        else if (!_myStatue.IsBroken)
        {
            _animator.Play(_crackedAnimation);
        }
            
    }
    public void BreakStatue()
    {
        if (_myStatue.IsBroken)
            return;
        _myStatue.IsBroken = true;
        _animator.Play(_breakAnimation);
    }

}
