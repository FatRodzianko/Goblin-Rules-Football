using System.Collections;
using UnityEngine;

// Offline decoration for the title screen. The cowboy leans against the wall until something
// hides him from view, then walks his waypoints once and leans again at the far end. No
// networking - the gameplay cowboy is CowboyScript, a NetworkBehaviour.
public class TitleScreenCowboy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private Transform _cigaretteSmoke;
    [SerializeField] private ParticleSystem _exhaleSmoke;

    [Header("Walking")]
    [Tooltip("Walked in order, once per scene load. He starts where he is placed and stops at the last one.")]
    [SerializeField] private Transform[] _waypoints = new Transform[0];
    [SerializeField] private float _walkSpeed = 1.25f;

    [Tooltip("Panels that cover him. He sets off the first time any of them is showing. Leave empty to drive StartWalking() from a menu entry's Activated event instead.")]
    [SerializeField] private GameObject[] _hiddenBy = new GameObject[0];

    [Header("Leaning")]
    [Tooltip("Wait before the first lean, so the animator is up before the state is asked for.")]
    [SerializeField] private float _firstLeanDelay = 0.5f;

    [Tooltip("He holds still for this long after each exhale, so the smoking does not run at the clip's own rate.")]
    [SerializeField] private float _minExhalePause = 2f;
    [SerializeField] private float _maxExhalePause = 5f;

    private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
    private static readonly int IsLeaningHash = Animator.StringToHash("isLeaning");

    private const float ArriveDistance = 0.02f;

    private int _targetIndex;
    private bool _isWalking;
    private bool _hasWalked;
    private Coroutine _firstLean;
    private Coroutine _exhalePause;

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        StartLeaning(-1f);
    }

    private void Update()
    {
        if (!_isWalking)
        {
            if (!_hasWalked && IsHidden())
                StartWalking();

            return;
        }

        Vector3 target = _waypoints[_targetIndex].position;
        Vector3 toTarget = target - transform.position;
        float step = _walkSpeed * Time.deltaTime;

        if (toTarget.magnitude <= Mathf.Max(step, ArriveDistance))
        {
            transform.position = target;
            _targetIndex++;
            if (_targetIndex >= _waypoints.Length)
                ArriveAtLastWaypoint();

            return;
        }

        transform.position += toTarget.normalized * step;
        FaceDirection(toTarget.x);
    }

    /// <summary>
    /// Sends him on his way. Safe to call more than once and from a menu entry's Activated event -
    /// he only ever makes the walk once per scene load.
    /// </summary>
    public void StartWalking()
    {
        if (_hasWalked || _waypoints.Length == 0)
            return;

        _hasWalked = true;
        _isWalking = true;
        _targetIndex = 0;

        CancelFirstLean();

        // A paused animator does not run transitions either, so he has to be resumed before the
        // bools below mean anything.
        ResumeAnimator();

        // Any State pulls him back into leaning for as long as this is set, so it has to go first.
        _animator.SetBool(IsLeaningHash, false);
        _animator.SetBool(IsWalkingHash, true);
    }

    // Called by an animation event partway through the leaning clip. Holding the animator still
    // afterwards stretches the smoke out: the clip resumes from the frame it stopped on, so the
    // exhale reads as a long drag rather than the clip's own 2.75s loop.
    public void ExhaleSmoke()
    {
        if (_exhaleSmoke != null)
            _exhaleSmoke.Play();

        if (_isWalking)
            return;

        CancelExhalePause();
        _exhalePause = StartCoroutine(PauseAfterExhale());
    }

    private IEnumerator PauseAfterExhale()
    {
        _animator.speed = 0f;
        yield return new WaitForSeconds(Random.Range(_minExhalePause, _maxExhalePause));

        _animator.speed = 1f;
        _exhalePause = null;
    }

    // Called by an animation event at the end of each loop of the leaning clip. He leans for as
    // long as the title screen is up, so there is nothing to count down - but the clip fires this
    // by name, and a missing receiver is logged every loop.
    public void EndOfOneWait()
    {
    }

    private bool IsHidden()
    {
        for (int i = 0; i < _hiddenBy.Length; i++)
        {
            if (_hiddenBy[i] != null && _hiddenBy[i].activeInHierarchy)
                return true;
        }

        return false;
    }

    private void ArriveAtLastWaypoint()
    {
        _isWalking = false;
        _animator.SetBool(IsWalkingHash, false);
        StartLeaning(1f);
    }

    private void StartLeaning(float facing)
    {
        FaceDirection(facing);
        CancelFirstLean();
        _firstLean = StartCoroutine(LeanAfterDelay());
    }

    private IEnumerator LeanAfterDelay()
    {
        _animator.speed = 0f;
        _animator.SetBool(IsLeaningHash, true);
        
        yield return new WaitForSeconds(_firstLeanDelay);
        _animator.speed = 1f;

        _firstLean = null;
    }

    private void CancelFirstLean()
    {
        if (_firstLean == null)
            return;

        StopCoroutine(_firstLean);
        _firstLean = null;
    }

    private void CancelExhalePause()
    {
        if (_exhalePause == null)
            return;

        StopCoroutine(_exhalePause);
        _exhalePause = null;
    }

    // Animator.speed lives on the component, not on the coroutine, so a pause that is interrupted
    // would otherwise leave him frozen for good.
    private void ResumeAnimator()
    {
        CancelExhalePause();

        if (_animator != null)
            _animator.speed = 1f;
    }

    private void OnDisable()
    {
        ResumeAnimator();
    }

    private void FaceDirection(float xDirection)
    {
        if (Mathf.Approximately(xDirection, 0f) || _renderer == null)
            return;

        bool faceLeft = xDirection < 0f;
        if (_renderer.flipX == faceLeft)
            return;

        _renderer.flipX = faceLeft;
        Vector3 smokeScale = new Vector3(faceLeft ? -1f : 1f, 1f, 1f);
        if (_cigaretteSmoke != null)
            _cigaretteSmoke.localScale = smokeScale;
        if (_exhaleSmoke != null)
            _exhaleSmoke.transform.localScale = smokeScale;
    }

    // Draws the route in the scene view while the cowboy is selected.
    private void OnDrawGizmosSelected()
    {
        if (_waypoints == null || _waypoints.Length == 0)
            return;

        Gizmos.color = new Color(1f, 0.78f, 0.25f, 1f);
        Vector3 from = transform.position;
        for (int i = 0; i < _waypoints.Length; i++)
        {
            if (_waypoints[i] == null)
                continue;

            Gizmos.DrawLine(from, _waypoints[i].position);
            Gizmos.DrawWireSphere(_waypoints[i].position, 0.15f);
            from = _waypoints[i].position;
        }
    }
}
