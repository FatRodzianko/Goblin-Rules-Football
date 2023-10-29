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
    bool _isWaitForSongEndRoutineRunning = false;


    //[SerializeField] AudioSource audioSource;
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        MakeInstance();
        if (!this._source)
            this._source = GetComponent<AudioSource>();
        //foreach (SongClip s in songs)
        //{
        //    s.source = gameObject.AddComponent<AudioSource>();
        //    _source.clip = s.clip;
        //    _source.outputAudioMixerGroup = mixerGroup;
        //    _source.volume = s.volume;
        //    _source.pitch = s.pitch;
        //    _source.playOnAwake = false;
        //    _source.loop = s.isLooping;

        //}
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
        //this.PlaySong("music-frail-noise", 1f);
        //ShuffleSongList();
        //PlaySong(1f);
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
        //StopSound(_titleMusicClip);
        if (_isWaitForSongEndRoutineRunning)
            StopCoroutine(_waitForSongEndRoutine);
        if (_source.isPlaying)
        {
            _source.Stop();
        }
        try
        {
            SongClip s = songs[SongIdex - 1];
            if (s != null)
            {
                s.clip.UnloadAudioData();
            }
        }
        catch (Exception e)
        {
            Debug.Log("TurnMusicOff: Could not get clip from index. Error: " + e);
        }
        
    }
    public void TurnMusicOn()
    {
        //PlaySound(_titleMusicClip, 0.75f);
        PlayPlayList(false);
    }
    public bool IsMusicPlaying()
    {
        if (_source == null)
            return false;
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
        PlaySong(1f);
    }
    public void PlaySong(float volume)
    {
        Debug.Log("PlaySong: Song at index: " + this.SongIdex.ToString());
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
            
        //Debug.Log("PlaySound: " + name);
        if (!_source.enabled)
            _source.enabled = true;
        _source.volume = volume;


        //AsyncOperationHandle<AudioClip> loadSong = Addressables.LoadAssetAsync<AudioClip>(s.ClipAddress);
        //s.songAddressable = loadSong;
        //s.clip = loadSong.WaitForCompletion();

        s.clip.LoadAudioData();
        _source.clip = s.clip;
        
        _source.PlayOneShot(s.clip, volume);

        _waitForSongEndRoutine = WaitForSongEnd(SongIdex);
        StartCoroutine(_waitForSongEndRoutine);
        SongIdex++;
        
        //if (s.isLooping)
        //    _source.Play();
        //else
        //    _source.PlayOneShot(s.clip, volume);
    }

    void ReleaseSong(int index)
    {
        Debug.Log("ReleaseSong: " + index.ToString());
        SongClip s = songs[index];
        if (s == null)
            return;
        if (_source.isPlaying)
            _source.Stop();
        s.clip.UnloadAudioData();
        //Addressables.Release(s.songAddressable);
        PlaySong(1.0f);
    }
    IEnumerator WaitForSongEnd(int index)
    {
        _isWaitForSongEndRoutineRunning = true;
        yield return new WaitUntil(() => !_source.isPlaying && (_source.time == 0f));
        Debug.Log("WaitForSongEnd: Music Ended.");
        ReleaseSong(index);
        _isWaitForSongEndRoutineRunning = false;
    }
}
