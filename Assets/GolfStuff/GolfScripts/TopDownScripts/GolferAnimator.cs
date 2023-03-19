using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Component.Animating;
using FishNet;

public class GolferAnimator : NetworkBehaviour
{
    [Header("Animation Stuff")]
    [SerializeField] Animator _animator;
    [SerializeField] NetworkAnimator _networkAnimator;
    [SerializeField] string _idle;
    [SerializeField] string _frontSwing;
    [SerializeField] string _deathFromLightning;

    [Header("Player Sprite")]
    [SerializeField] SpriteRenderer _spriteRenderer;

    [Header("Animation State")]
    public bool IsSwinging = false;
    public bool IsDyingFromLightning = false;

    [Header("Player Owner")]
    [SerializeField] GolfPlayerTopDown _myPlayer;

    [Header("Struck By Lightning Stuff")]
    [SerializeField] Sprite _struckByLightningFullSwingFlashOn;
    [SerializeField] Sprite _struckByLightningFullSwingFlashOff;
    [SerializeField] GameObject _lightningBoltObject;
    // Start is called before the first frame update
    void Awake()
    {
        if(!_myPlayer)
            _myPlayer = this.transform.parent.GetComponent<GolfPlayerTopDown>();
        if(!_spriteRenderer)
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
        if (!IsOwner)
            return;
        if (IsSwinging)
            return;
        if (IsDyingFromLightning)
            return;

        AnimatorClipInfo[] animatorInfo = _animator.GetCurrentAnimatorClipInfo(0);
        if (animatorInfo[0].clip.name != _idle)
        {
            //Debug.Log("GolferAnimator : starting the idle animation");
            //_animator.Play(_idle);
            _networkAnimator.Play(_idle);
        }
    }
    public void StartSwing()
    {
        IsSwinging = true;
        //_animator.Play(_frontSwing);
        _networkAnimator.Play(_frontSwing);
    }
    public void HitBall()
    {
        Debug.Log("HitBall: " + this._myPlayer.PlayerName);
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
        if(InstanceFinder.IsServer) // only run on the server?
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
        if (IsDyingFromLightning)
            IsDyingFromLightning = false;
        if(!_animator.enabled)
            _animator.enabled = true;
    }
    public void ChangeToStruckByLightningSprite(bool lightningFlash)
    {
        if (lightningFlash)
        {
            _spriteRenderer.sprite = _struckByLightningFullSwingFlashOn;
            _lightningBoltObject.SetActive(true);
        }
        else
        {
            _spriteRenderer.sprite = _struckByLightningFullSwingFlashOff;
            _lightningBoltObject.SetActive(false);
        }   
    }
    public void StartDeathFromLightning()
    {
        ResetGolfAnimator();
        IsDyingFromLightning = true;
        _animator.Play(_deathFromLightning);
    }
    public void DeathFromLightningAnimOver()
    { 
        _myPlayer.PlayerUIMessage("struck by lightning");
        _myPlayer.EnablePlayerCanvas(true);
    }
}
