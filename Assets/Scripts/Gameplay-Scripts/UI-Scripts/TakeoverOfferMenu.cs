using UnityEngine;
using UnityEngine.UI;
using TMPro;

// The play-on prompt's UI, and nothing else. The decision flow lives in TakeoverManager on the server, and the
// messages between them ride GamePlayer's RPCs, so this is a plain MonoBehaviour on an always-active object under
// CanvasGameplay, like EscMenuManager - findable in the hierarchy, with the panel itself starting inactive.
// The buttons' On Click are wired in the Inspector to PlayOnPressed and LeaveGamePressed.
public class TakeoverOfferMenu : MonoBehaviour
{
    public static TakeoverOfferMenu instance;

    [SerializeField] GameObject OfferPanel;
    // The scroll child's ImageAnimation, configured like the settings and stats panels: the same unroll frames,
    // the content holder as its TextObjects, and PLAY ON as its FirstButton.
    [SerializeField] ImageAnimation ScrollAnimation;
    [SerializeField] GameObject ContentObjects;
    [SerializeField] TextMeshProUGUI OfferText;
    // The offer's own countdown. It cannot ride the shared timeout timer: that text sits under CanvasGameplay,
    // which is inactive until kickoff, so an offer during the coin toss counted down invisibly.
    [SerializeField] TextMeshProUGUI CountdownText;
    [SerializeField] Button PlayOnButton;
    [SerializeField] Button LeaveGameButton;

    void Awake()
    {
        instance = this;
    }
    public void ShowOffer(bool show)
    {
        if (OfferPanel == null)
            return;
        if (show && OfferText != null)
            OfferText.text = "The other team has left the game." + System.Environment.NewLine + "Play on against AI?";
        if (show && CountdownText != null)
            CountdownText.text = ":30"; // What OfferCountdownRoutine starts from; the SyncVar hook takes over from the first tick.
        if (ScrollAnimation == null)
        {
            OfferPanel.SetActive(show);
            return;
        }
        if (show)
        {
            // The scroll unrolls the way every other menu's does. ImageAnimation activates ContentObjects - the
            // text and buttons - when the unroll finishes, and selects PLAY ON for gamepad navigation. It runs on
            // unscaled time, so it plays while the offer has the game paused.
            if (ContentObjects != null)
                ContentObjects.SetActive(false);
            ScrollAnimation.UnScrollHalfTime();
        }
        else
        {
            // ResetMenu is the instant close the esc menu uses: sprite back to frame 0, scroll and panel off. The
            // content is forced off afterwards because ResetMenu's first-ever call can leave it on.
            ScrollAnimation.ResetMenu();
            if (ContentObjects != null)
                ContentObjects.SetActive(false);
        }
    }
    public void SetCountdown(int secondsLeft)
    {
        if (CountdownText != null)
            CountdownText.text = ":" + secondsLeft.ToString();
    }
    // The answer goes to the server through the local player's own ServerRpc, which is also what identifies whose
    // answer it is.
    public void PlayOnPressed()
    {
        SendAnswer(true);
    }
    public void LeaveGamePressed()
    {
        SendAnswer(false);
    }
    void SendAnswer(bool playOn)
    {
        GameObject localPlayerObject = GameObject.FindGameObjectWithTag("LocalGamePlayer");
        if (localPlayerObject == null)
            return;
        GamePlayer localPlayer = localPlayerObject.GetComponent<GamePlayer>();
        if (localPlayer != null)
            localPlayer.CmdAnswerTakeoverOffer(playOn);
    }
}
