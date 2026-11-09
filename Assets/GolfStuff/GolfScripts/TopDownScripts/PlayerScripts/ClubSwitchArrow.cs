using UnityEngine;
using UnityEngine.EventSystems;

// One of the two arrows beside the club name. It knows only which way it points and passes that to
// the indicator, which has no idea what a club is either.
public class ClubSwitchArrow : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ClubSwitchIndicator _indicator;
    [SerializeField] private int _direction = 1;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        if (!_indicator)
            return;

        _indicator.RequestSwitch(_direction);
    }
}
