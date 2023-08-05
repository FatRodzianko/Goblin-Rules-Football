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
    [SerializeField] string _pauseOnLightningStrike;
    [SerializeField] string _struckSkeleton;
    [SerializeField] string _swingStruckByLightning;

    [Header("Player Sprite")]
    [SerializeField] SpriteRenderer _spriteRenderer;

    [Header("Animation State")]
    public bool IsSwinging = false;
    public bool IsDyingFromLightning = false;
    public bool IsPausedOnLightningStrike = false;

    [Header("Player Owner")]
    [SerializeField] GolfPlayerTopDown _myPlayer;

    [Header("Struck By Lightning Stuff")]
    [SerializeField] Sprite _struckByLightningFullSwingFlashOn;
    [SerializeField] Sprite _struckByLightningFullSwingFlashOff;
    [SerializeField] GameObject _lightningBoltObject;
    LightningManager _lightningManager;

    // Start is called before the first frame update
    void Awake()
    {
        if(!_myPlayer)
            _myPlayer = this.transform.parent.GetComponent<GolfPlayerTopDown>();
        if(!_spriteRenderer)
            _spriteRenderer = this.GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        _lightningManager = GameplayManagerTopDownGolf.instance.GetLightningManager();
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
        if (IsPausedOnLightningStrike)
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
        //_networkAnimator.Play(_frontSwing);
        //_networkAnimator.Animator.Play(_frontSwing);
        _networkAnimator.SetTrigger("front swing");
        Debug.Log("StartSwing: _networkAnimator.SetTrigger(\"front swing\");");
    }
    public void HitBall()
    {
        if (!this.IsOwner)
            return;
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
        IsPausedOnLightningStrike = true;
        //_networkAnimator.SetTrigger("PauseForLightning");
        //_networkAnimator.Play(_pauseOnLightningStrike);

        //old animation call
        //_networkAnimator.SetTrigger("StruckSkeleton");

        //_networkAnimator.Play(_swingStruckByLightning);
        _networkAnimator.SetTrigger("struck by lightning");
        //_animator.enabled = false;

    }
    public void PlayerStruckByLightningForClients()
    {
        IsPausedOnLightningStrike = true;
        _animator.Play(_pauseOnLightningStrike);
        
    }
    public void ResetGolfAnimator()
    {
        if (IsSwinging)
            IsSwinging = false;
        if (IsDyingFromLightning)
            IsDyingFromLightning = false;
        if(!_animator.enabled)
            _animator.enabled = true;
        if (IsPausedOnLightningStrike)
            IsPausedOnLightningStrike = false;
    }
    public void ChangeToStruckByLightningSprite(bool lightningFlash)
    {
        Debug.Log("ChangeToStruckByLightningSprite: " + lightningFlash.ToString());
        if (lightningFlash)
        {
            //_spriteRenderer.sprite = _struckByLightningFullSwingFlashOn;
            //_lightningBoltObject.SetActive(true);
            //_networkAnimator.SetTrigger("StruckSkeleton");
            _networkAnimator.Play(_struckSkeleton);
        }
        else
        {
            //_spriteRenderer.sprite = _struckByLightningFullSwingFlashOff;
            //_lightningBoltObject.SetActive(false);
            //_networkAnimator.SetTrigger("PauseForLightning");
            _networkAnimator.Play(_pauseOnLightningStrike);
        }   
    }
    public void StartDeathFromLightning()
    {
        Debug.Log("StartDeathFromLightning");
        ResetGolfAnimator();
        IsDyingFromLightning = true;
        //_animator.Play(_deathFromLightning);
        if (this.IsOwner)
        {
            _networkAnimator.Play(_deathFromLightning);
            //_animator.Play(_deathFromLightning);
            Debug.Log("StartDeathFromLightning: playing animation _deathFromLightning from owner.");
        }
            
    }
    public void DeathFromLightningAnimOver()
    {
        if (this.IsOwner)
        {
            _myPlayer.PlayerUIMessage("struck by lightning");
            _myPlayer.EnablePlayerCanvas(true);
        }
    }
    public void TurnOnLightForLightningFlash()
    {
        _lightningManager.TurnOnLightForLightningStrike(_lightningManager.LightIntensityMax);
    }
    public void TurnOffLightForLightningFlash()
    {
        _lightningManager.TurnOffLightForLightningStrike();
    }
    public void ThunderForLightningStrike()
    {
        _lightningManager.PlayThunderClip(true, 0f);
    }
}
