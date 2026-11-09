using UnityEngine;
using UnityEngine.EventSystems;

// The spin selector on the player's canvas. The player owns the spin value; this turns a pointer
// position or an arrow-key nudge into a normalised -1..1 vector and hands that over, so nothing
// outside this script needs to know how big the icon is.
//
// Pointer input goes through the canvas raycaster rather than a collider. ScreenPointToLocalPointInRectangle
// with the event's own camera is correct both for Screen Space - Overlay and for the Screen Space -
// Camera the CRT presenter converts canvases to at runtime.
public class SpinIcon : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerClickHandler
{
    [SerializeField] GolfPlayerTopDown _myPlayer;
    [SerializeField] RectTransform _myRect;
    [SerializeField] RectTransform _selectionIcon;

    // How close to the edge of the icon the selection dot may get, in canvas pixels.
    [SerializeField] float _edgeMargin = 2f;
    public float _maxDistanceFromCenter;

    [Header("Icon Movement Stuff")]
    [SerializeField] float _moveRate = 32f;
    [SerializeField] Vector2 _moveSpinDirection = Vector2.zero;
    [SerializeField] bool _moveIcon = false;

    void Start()
    {
        if (!_myRect)
            _myRect = transform as RectTransform;

        _maxDistanceFromCenter = (_myRect.rect.width * 0.5f) - _edgeMargin;
    }
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
        SetSpinFromPoint(_selectionIcon.anchoredPosition + _moveSpinDirection * _moveRate * Time.deltaTime);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        SetSpinFromPointer(eventData);
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        SetSpinFromPointer(eventData);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
            return;
        if (!CanPlayerSetSpin())
            return;

        ResetIconPosition();
    }
    void SetSpinFromPointer(PointerEventData eventData)
    {
        if (!CanPlayerSetSpin())
            return;

        Vector2 pointInIcon;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_myRect, eventData.position, eventData.pressEventCamera, out pointInIcon))
            return;

        SetSpinFromPoint(pointInIcon);
    }
    void SetSpinFromPoint(Vector2 pointInIcon)
    {
        Vector2 clamped = Vector2.ClampMagnitude(pointInIcon, _maxDistanceFromCenter);
        _selectionIcon.anchoredPosition = clamped;
        _myPlayer.UpdateHitSpinForPlayer(AdjustVectorForSpin(clamped));
    }
    bool CanPlayerSetSpin()
    {
        if (!_myPlayer)
            return false;
        if (!_myPlayer.IsOwner)
            return false;

        return !_myPlayer.DirectionAndDistanceChosen;
    }
    public void ResetIconPosition()
    {
        _selectionIcon.anchoredPosition = Vector2.zero;
        _myPlayer.UpdateHitSpinForPlayer(Vector2.zero);
    }
    Vector2 AdjustVectorForSpin(Vector2 newSpin)
    {
        if (_maxDistanceFromCenter <= 0f)
            return Vector2.zero;

        return newSpin / _maxDistanceFromCenter;
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
