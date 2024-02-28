using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NowPlayingMusicText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI NowPlayingText;

    // Start is called before the first frame update
    void Start()
    {
        if (!NowPlayingText)
        {
            Debug.Log("NowPlayingMusicText: no text object set?");
            return;
        }
            
        MusicManager.instance.MusicTurnedOn += UpdateIsMusicPlaying;
        MusicManager.instance.NowPlayingSongChanged += UpdateNowPlayingSongTitle;

        // Check to see if a song is currently playing. Used when player had a song playing but then loads a new scene, such as quitting out of a game and returning to the title screen
        if (MusicManager.instance.IsMusicPlaying())
        {
            UpdateIsMusicPlaying(true);
            UpdateNowPlayingSongTitle(MusicManager.instance.GetCurrentSongTitle());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void UpdateIsMusicPlaying(bool music)
    {
        Debug.Log("UpdateIsMusicPlaying: " + music);
        this.NowPlayingText.enabled = music;
        if (!music)
            this.NowPlayingText.text = "";
        //this.gameObject.SetActive(music);
    }
    void UpdateNowPlayingSongTitle(string songTitle)
    {
        Debug.Log("UpdateNowPlayingSongTitle: " + songTitle);
        if (string.IsNullOrEmpty(songTitle))
            return;

        string nowPlayingText = songTitle.Replace("-"," ");
        NowPlayingText.text = "Now Playing: " + nowPlayingText;
    }
    private void OnDestroy()
    {
        MusicManager.instance.MusicTurnedOn -= UpdateIsMusicPlaying;
        MusicManager.instance.NowPlayingSongChanged -= UpdateNowPlayingSongTitle;
    }
}
