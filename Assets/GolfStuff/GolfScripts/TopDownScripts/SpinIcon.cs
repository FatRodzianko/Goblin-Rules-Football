using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinIcon : MonoBehaviour
{
    [SerializeField] GolfPlayerTopDown _myPlayer;
    [SerializeField] GameObject _selectionIconObject;
    [SerializeField] CircleCollider2D _myCollider;
    [SerializeField] LayerMask _spinIconLayerMask;
    public float _maxDistanceFromCenter;

    [Header("Icon Movement Stuff")]
    [SerializeField] float _moveRate = 2f;
    [SerializeField] Vector2 _moveSpinDirection = Vector2.zero;
    [SerializeField] bool _moveIcon = false;

    // Start is called before the first frame update
    void Start()
    {
        _maxDistanceFromCenter = _myCollider.radius - (_myPlayer.MyBall.pixelUnit * 2f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_moveIcon)
            return;
        if (_moveSpinDirection == Vector2.zero)
            return;
        if (_myPlayer.DirectionAndDistanceChosen)
            return;
        MoveSpinIcon();
    }
    void MoveSpinIcon()
    {
        _selectionIconObject.transform.localPosition = Vector2.ClampMagnitude(((Vector2)_selectionIconObject.transform.localPosition + _moveSpinDirection * _moveRate * Time.deltaTime), _maxDistanceFromCenter);
        _myPlayer.UpdateHitSpinForPlayer(AdjustVectorForSpin(_selectionIconObject.transform.localPosition));
    }
    private void OnMouseDown()
    {
        if (!_myPlayer.IsOwner)
            return;
        if (_myPlayer.DirectionAndDistanceChosen)
            return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _selectionIconObject.transform.position = new Vector3(mousePos.x, mousePos.y, 0f);
        _myPlayer.UpdateHitSpinForPlayer(AdjustVectorForSpin(_selectionIconObject.transform.localPosition));
    }
    private void OnMouseDrag()
    {
        if (!_myPlayer.IsOwner)
            return;
        if (_myPlayer.DirectionAndDistanceChosen)
            return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Vector2.Distance(mousePos, this.transform.position) < _maxDistanceFromCenter)
        {
            _selectionIconObject.transform.position = new Vector3(mousePos.x, mousePos.y, 0f);
        }
        else
        {
            Vector2 startPos = this.transform.position;
            Vector2 dir = ((Vector2)mousePos - startPos).normalized;
            Vector2 endPos = startPos + dir * _maxDistanceFromCenter;
            _selectionIconObject.transform.position = new Vector3(endPos.x, endPos.y, 0f);
        }


        _myPlayer.UpdateHitSpinForPlayer(AdjustVectorForSpin(_selectionIconObject.transform.localPosition));
        
    }
    private void OnMouseOver()
    {
        if (!_myPlayer.IsOwner)
            return;
        if (_myPlayer.DirectionAndDistanceChosen)
            return;
        if (Input.GetMouseButtonDown(1))
            ResetIconPosition();
    }
    public void ResetIconPosition()
    {
        _selectionIconObject.transform.localPosition = Vector2.zero;
        _myPlayer.UpdateHitSpinForPlayer(AdjustVectorForSpin(_selectionIconObject.transform.localPosition));
    }
    Vector2 AdjustVectorForSpin(Vector2 newSpin)
    {
        Vector2 spin = Vector2.zero;
        if (newSpin.y != 0f)
        {
            spin.y = newSpin.y / _maxDistanceFromCenter;
        }
        if (newSpin.x != 0f)
        {
            spin.x = newSpin.x / _maxDistanceFromCenter;
        }
        return spin;
    }
    public void UpdateSpinDirection(Vector2 dir)
    {
        
        _moveSpinDirection = dir;
        _moveIcon = true;
    }
    public void ResetSpinDirection()
    {
        _moveSpinDirection = Vector2.zero;
        _moveIcon = false;
    }

}
