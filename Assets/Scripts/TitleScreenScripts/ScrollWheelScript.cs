/// <summary>
/// Scrolls a scroll rect's content based on the public scroll speed.
///
/// Notes:
/// - must include IScrollHandler in the inheritance for OnScroll event.
/// - assumes that the panel starts at 0,0,0 (for min/max scrolling)
/// - assumes that the panel will only be scrolled vertically
///
/// Created By: Matt Barr
///     ClearWave Interactive, LLC
/// </summary>

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollWheelScript : MonoBehaviour, IScrollHandler
{
    RectTransform TransRef;
    ScrollRect ScrollRef;
    RectTransform ContentRef;

    [SerializeField] float ScrollSpeed = 100;

    float MinScroll = 0;
    float MaxScroll;

    // Use this for initialization
    void Start()
    {
        TransRef = GetComponent<RectTransform>();
        ScrollRef = GetComponent<ScrollRect>();
        ContentRef = ScrollRef.content;

        MaxScroll = ContentRef.rect.height - TransRef.rect.height;
    }

    public void OnScroll(PointerEventData eventData)
    {
        Vector2 ScrollDelta = eventData.scrollDelta;

        ContentRef.anchoredPosition += new Vector2(0, -ScrollDelta.y * ScrollSpeed);

        if (ContentRef.anchoredPosition.y < MinScroll)
        {
            ContentRef.anchoredPosition = new Vector2(0, MinScroll);
        }
        else if (ContentRef.anchoredPosition.y > MaxScroll)
        {
            ContentRef.anchoredPosition = new Vector2(0, MaxScroll);
        }
    }
}