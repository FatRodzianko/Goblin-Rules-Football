using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Managing;
using FishNet.Object.Synchronizing;
using FishNet;

public class Statue : NetworkBehaviour
{

    [Header("Sprite Stuff")]
    [SerializeField] SpriteRenderer _myRenderer;
    [SerializeField] Sprite _mySprite;
    [SerializeField] StatueAnimator _statueAnimator;

    [Header("Statue Info")]
    [SerializeField] public string StatueType;
    [SerializeField] public float HeightInUnityUnits;
    [SerializeField] float _brokenHeight;
    [SerializeField] float _originalHeight;
    [SerializeField] [SyncVar(OnChange = nameof(SyncRingRadius))] public float RingRadius;
    [SerializeField] public bool IsBroken;

    [Header("Collider Stuff")]
    [SerializeField] Collider2D _myCollider;
    [SerializeField] CircleCollider2D _ringRadiusCollider;
    [SerializeField] EnvironmentObstacleTopDown _myEnvironmentObstacleTopDownScript;

    [Header("Ring Components")]
    [SerializeField] LineRenderer _myLineRenderer;
    [SerializeField] float _lineWidth;
    [SerializeField] int _lineSegments;

    [Header("Inner Circle Components")]
    [SerializeField] GameObject _innerCircle;

    // Start is called before the first frame update
    void Start()
    {
        //RingRadius = GetStartingRadius();
        //_lineSegments = GetRingSegments(RingRadius);
        //SetLineThickness(RingRadius);
        //DrawCircle(_lineSegments, RingRadius);
        //UpdateInnerCircleSize(RingRadius);
        HeightInUnityUnits = _originalHeight;
        _myEnvironmentObstacleTopDownScript.HeightInUnityUnits = HeightInUnityUnits;
        if (!_statueAnimator)
            _statueAnimator = this.transform.GetComponent<StatueAnimator>();
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        RingRadius = GetStartingRadius();
    }
    void SyncRingRadius(float prev, float next, bool asServer)
    {
        if (asServer)
        {
            return;
        }
        UpdateStatueCircle(next);
    }
    void UpdateStatueCircle(float newRadius)
    {
        _lineSegments = GetRingSegments(newRadius);
        SetLineThickness(newRadius);
        DrawCircle(_lineSegments, newRadius);
        UpdateInnerCircleSize(newRadius);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    float GetStartingRadius()
    {
        if(this.StatueType == "good-weather")
            return UnityEngine.Random.Range(3f, 7.5f);
        else
            return UnityEngine.Random.Range(8f, 12.5f);
    }
    int GetRingSegments(float radius)
    {
        return (int)(radius * 10);
    }
    void SetLineThickness(float radius)
    {
        _lineWidth = radius * 0.03f;
        _myLineRenderer.startWidth = _lineWidth;
        _myLineRenderer.endWidth = _lineWidth;
    }
    void UpdateInnerCircleSize(float radius)
    {
        _innerCircle.transform.localScale = new Vector3(radius * 2, radius * 2, 1f);
    }
    [ObserversRpc(BufferLast = true)]
    public void RpcUpdatePosition(Vector3 newPos)
    {
        this.transform.position = newPos;
        _innerCircle.SetActive(true);
    }
    void DrawCircle(int steps, float radius)
    {

        _myLineRenderer.positionCount = steps + 1;
        Vector3 myPos = this.transform.localPosition;
        for (int currentStep = 0; currentStep < steps; currentStep++)
        {
            float circumferenceProgress = (float)currentStep / steps;

            float currentRadian = circumferenceProgress * 2 * Mathf.PI;
            float xScaled = Mathf.Cos(currentRadian);
            float yScaled = Mathf.Sin(currentRadian);
            float x = xScaled * radius;
            float y = yScaled * radius;

            Vector3 currentPos = new Vector3(x, y, 0);
            //currentPos += myPos;


            _myLineRenderer.SetPosition(currentStep, currentPos);
        }

        _myLineRenderer.SetPosition(_myLineRenderer.positionCount - 1, _myLineRenderer.GetPosition(0));
        
    }
    public void BreakStatueAnimation()
    {
        CmdBreakStatueAnimation();
        _statueAnimator.BreakStatue();
    }
    [ServerRpc(RequireOwnership = false)]
    void CmdBreakStatueAnimation()
    {
        RpcBreakStatueAnimation();
    }
    [ObserversRpc(BufferLast = true)]
    void RpcBreakStatueAnimation()
    {
        _statueAnimator.BreakStatue();
    }
    public void DisableRingRadiusForBrokenStatue()
    {
        _innerCircle.SetActive(false);
        _myLineRenderer.enabled = false;
        SetBrokenStatueHeight();
    }
    void SetBrokenStatueHeight()
    {
        HeightInUnityUnits = _brokenHeight;
        _myEnvironmentObstacleTopDownScript.HeightInUnityUnits = HeightInUnityUnits;
    }
}
