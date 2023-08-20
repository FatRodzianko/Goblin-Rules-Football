using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningHoop : MonoBehaviour
{
    [SerializeField] public  CircleCollider2D TopHoopCollider;

    [Header("Ball Collision Stuff")]
    [SerializeField] bool _inBaseCollider = false;
    [SerializeField] bool _inTopCollider = false;
    [SerializeField] public bool BallAlreadyWentThroughHoop = false;

    [Header("Sprite Stuff")]
    [SerializeField] SpriteRenderer _mySpriteRenderer;
    [SerializeField] SpriteMask _mySpriteMask;

    [Header("Animation")]
    [SerializeField] Animator _myAnimator;

    // Start is called before the first frame update
    void Start()
    {
        if (!_myAnimator)
            _myAnimator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void BallEnteredBaseCollider()
    { 

    }
    public void BallPassedThroughHoop(GolfBallTopDown golfBallScript)
    {
        if (BallAlreadyWentThroughHoop)
            return;
        BallAlreadyWentThroughHoop = true;
        golfBallScript.MyPlayer.BallThroughHoop();
        StartCoroutine(SpeedUpSpinAnimation());
    }
    public void UpdateSpriteMask()
    {
        // this results in the spritemask being the previous sprite for whatever reason. Have it pull from an array? take an index + 1?
        _mySpriteMask.sprite = _mySpriteRenderer.sprite;
    }
    IEnumerator SpeedUpSpinAnimation()
    {
        _myAnimator.speed = 5f;
        yield return new WaitForSeconds(2f);
        _myAnimator.speed = 4f;
        yield return new WaitForSeconds(0.75f);
        _myAnimator.speed = 3f;
        yield return new WaitForSeconds(0.75f);
        _myAnimator.speed = 2f;
        yield return new WaitForSeconds(0.75f);

        _myAnimator.speed = 1f;
        BallAlreadyWentThroughHoop = false;
    }
}
