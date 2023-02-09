using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolferAnimator : MonoBehaviour
{
    [Header("Animation Stuff")]
    [SerializeField] Animator _animator;
    [SerializeField] string _idle;
    [SerializeField] string _frontSwing;

    [Header("Player Sprite")]
    [SerializeField] SpriteRenderer _spriteRenderer;

    [Header("Animation State")]
    public bool IsSwinging = false;

    [Header("Player Owner")]
    [SerializeField] GolfPlayerTopDown _myPlayer;
    // Start is called before the first frame update
    void Awake()
    {
        _myPlayer = this.transform.parent.GetComponent<GolfPlayerTopDown>();
        _spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        /*if (!IsSwinging)
        {
            AnimatorClipInfo[] animatorInfo = _animator.GetCurrentAnimatorClipInfo(0);
            if (animatorInfo[0].clip.name != _idle)
            {
                //Debug.Log("GolferAnimator : starting the idle animation");
                _animator.Play(_idle);
            }
            
        }*/
        if (IsSwinging)
            return;

        AnimatorClipInfo[] animatorInfo = _animator.GetCurrentAnimatorClipInfo(0);
        if (animatorInfo[0].clip.name != _idle)
        {
            //Debug.Log("GolferAnimator : starting the idle animation");
            _animator.Play(_idle);
        }
    }
    public void StartSwing()
    {
        IsSwinging = true;
        _animator.Play(_frontSwing);
    }
    public void HitBall()
    {
        _myPlayer.SubmitHitToBall();
    }
    public void SwingEnded()
    {
        IsSwinging = false;
    }
    public void UpdateSpriteDirection(Vector2 hitDirection)
    {
        if (hitDirection.x < 0)
            _spriteRenderer.flipX = true;
        else
            _spriteRenderer.flipX = false;
    }
    public void LightningStrikeForHit()
    {
        GameplayManagerTopDownGolf.instance.LightningForPlayerHit(_myPlayer);
    }
    public void EnablePlayerSprite(bool enable)
    {
        _spriteRenderer.enabled = enable;
    }
    public void PlayerStruckByLightning()
    {
        Debug.Log("GolferAnimator: PlayerStruckByLightning: Stopping the animation");
        _animator.enabled = false;
    }
    public void ResetGolfAnimator()
    {
        if (IsSwinging)
            IsSwinging = false;
        if(!_animator.enabled)
            _animator.enabled = true;
    }
}
