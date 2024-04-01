using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Linq;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    public AudioMixer audioMixer;
    public AudioMixerGroup mixerGroup;
    public SongClip[] songs;
    public int SongIdex = 0;
    [SerializeField] AudioSource _source;
    IEnumerator _waitForSongEndRoutine;
    [SerializeField] bool _isWaitForSongEndRoutineRunning = false;

    public delegate void ChangeNowPlayingSong(string nowPlayingSongTitle);
    public event ChangeNowPlayingSong NowPlayingSongChanged;

    public delegate void IsMusicOn(bool isMusicOn);
    public event IsMusicOn MusicTurnedOn;

    float _timeLastTurnedOff = 0f;

    [SerializeField] bool _playerTurnedMusicOn = false;

    // Audio Queue?
    Queue<AudioClip> _clipQueue = new Queue<AudioClip>();


    //[SerializeField] AudioSource audioSource;
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        MakeInstance();
        if (!this._source)
            this._source = GetComponent<AudioSource>();

        // Now Playing Events?
        MusicTurnedOn = MusicTurnedOnFunction;
        NowPlayingSongChanged = NowPlayingSongChangedFunction;
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {

    }
    void ShuffleSongList()
    {
        Debug.Log("ShuffleSongList: Starting first song: " + songs[0].name);
        List<SongClip> songList = new List<SongClip>();
        songList.AddRange(songs);
        System.Random rng = new System.Random();
        songs = songList.OrderBy(a => rng.Next()).ToArray();
        Debug.Log("ShuffleSongList: new first song: " + songs[0].name);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void TurnMusicOff()
    {
        if (_isWaitForSongEndRoutineRunning)
        {
            Debug.Log("TurnMusicOff: _isWaitForSongEndRoutineRunning was: " + _isWaitForSongEndRoutineRunning.ToString() + " (true) so stopping the routine.");
            StopCoroutine(_waitForSongEndRoutine);
            _isWaitForSongEndRoutineRunning = false;
        }
            
        if (_source.isPlaying)
        {
            Debug.Log("TurnMusicOff: _source.isPlaying is true. Doing _source.Stop()...");
            _source.Stop();
        }
        try
        {
            if (songs.Length > 0)
            {
                SongClip s = songs[SongIdex - 1];
                if (s != null)
                {
                    s.clip.UnloadAudioData();
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("TurnMusicOff: Could not get clip from index. Error: " + e);
        }
        MusicTurnedOn(false);
        _playerTurnedMusicOn = false;
        _timeLastTurnedOff = Time.time;
    }
    public void TurnMusicOn()
    {
        if (Time.time <= _timeLastTurnedOff + 0.15f)
        {
            Debug.Log("TurnMusicOn: Too soon after _timeLastTurnedOff. current time: " + Time.time + " _timeLastTurnedOff: " + _timeLastTurnedOff.ToString());
            return;
        }
        _playerTurnedMusicOn = true;
        PlayPlayList(false);
        MusicTurnedOn(true);
    }
    public bool IsMusicPlaying()
    {
        if (_source == null)
        {
            Debug.Log("IsMusicPlaying: no source for the music? Returning false. Weird...");
            return false;
        }   
        if (_source.clip == null && _source.isPlaying)
        {
            Debug.Log("IsMusicPlaying: clip is null and is music play?: " + _source.isPlaying.ToString());
            return false;
        }
        Debug.Log("IsMusicPlaying: " + _source.isPlaying.ToString());
        return _source.isPlaying;
    }
    void PlayPlayList(bool shuffle = false)
    {
        Debug.Log("PlayPlayList: shuffle?: " + shuffle.ToString());
        if (shuffle)
        {
            //ShuffleSongList();
        }
        Debug.Log("PlayPlayList: Calling PlaySong()");
        PlaySong(1f);
    }
    public void PlaySong(float volume)
    {
        Debug.Log("PlaySong: Song at index: " + this.SongIdex.ToString() + " player wants to play a song?: " + _playerTurnedMusicOn.ToString());
        if (!_playerTurnedMusicOn)
        {
            Debug.Log("PlaySong: _playerTurnedMusicOn is: " + _playerTurnedMusicOn.ToString() + " (should be false). Not playing the next song since the player seems to have turned it off and something bad happened?");
            return;
        }
        if (SongIdex >= songs.Length)
        {
            Debug.Log("PlaySong: Resetting index to 0?");
            ShuffleSongList();
            SongIdex = 0;
        }
            
        SongClip s = songs[SongIdex];
        if (s == null)
        {
            Debug.Log("PlaySong: s is null. Returning." );
            return;
        }
            
        if (!_source.enabled)
            _source.enabled = true;

        _source.volume = volume;
        s.clip.LoadAudioData();
        _source.clip = s.clip;

        //_source.PlayOneShot(s.clip, volume);
        _source.Play();

        NowPlayingSongChanged(s.name);

        _waitForSongEndRoutine = WaitForSongEnd(SongIdex);
        StartCoroutine(_waitForSongEndRoutine);
        SongIdex++;
    }

    void ReleaseSong(int index)
    {
        Debug.Log("ReleaseSong: " + index.ToString());
        if (songs.Length <= 0)
        {
            Debug.Log("ReleaseSong: The songs array has a length of 0? Returning...");
            return;
        }
        if (_source.isPlaying)
            _source.Stop();

        SongClip s = songs[index];
        if (s == null)
            return;        
        s.clip.UnloadAudioData();
        Debug.Log("ReleaseSong: Calling PlaySong()");
        PlaySong(1.0f);
    }
    IEnumerator WaitForSongEnd(int index)
    {
        Debug.Log("WaitForSongEnd: starting routine for song: " + _source.clip.name + " which has a clip length of: " + _source.clip.length.ToString() + " at index: " + index.ToString());
        _isWaitForSongEndRoutineRunning = true;
        yield return new WaitUntil(() => !_source.isPlaying && (_source.time == 0f));
        Debug.Log("WaitForSongEnd: Music Ended for clip: " + _source.clip.name + ".");
        _isWaitForSongEndRoutineRunning = false;
        ReleaseSong(index);
    }
    void MusicTurnedOnFunction(bool isMusicOn)
    {
        Debug.Log("MusicTurnedOnFunction: " + isMusicOn.ToString());
    }
    void NowPlayingSongChangedFunction(string nowPlayingSongTitle)
    {
        Debug.Log("NowPlayingSongChangedFunction: " + nowPlayingSongTitle);
    }
    public string GetCurrentSongTitle()
    {
        if (!this.IsMusicPlaying())
            return "";

        int index = 0;
        if (SongIdex > 0)
            index = SongIdex;
        if (index > songs.Length)
            index = songs.Length;

        SongClip s = songs[index-1];
        return s.name;
    }
}
