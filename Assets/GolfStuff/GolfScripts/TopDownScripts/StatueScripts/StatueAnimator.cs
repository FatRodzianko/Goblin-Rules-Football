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

    // Start is called before the first frame update
    void Start()
    {
        if (!_myStatue)
            _myStatue = this.transform.GetComponent<Statue>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void BreakStatue()
    {
        if (_myStatue.IsBroken)
            return;
        _animator.Play(_breakAnimation);
        _myStatue.IsBroken = true;
    }
}
