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


    //[SerializeField] AudioSource audioSource;
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        MakeInstance();

        foreach (SongClip s in songs)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.outputAudioMixerGroup = mixerGroup;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.playOnAwake = false;
            s.source.loop = s.isLooping;

        }
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
        ShuffleSongList();
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
        if (!s.source.enabled)
            s.source.enabled = true;
        s.source.volume = volume;


        //AsyncOperationHandle<AudioClip> loadSong = Addressables.LoadAssetAsync<AudioClip>(s.ClipAddress);
        //s.songAddressable = loadSong;
        //s.clip = loadSong.WaitForCompletion();
        s.clip.LoadAudioData();
        s.source.clip = s.clip;
        
        s.source.PlayOneShot(s.clip, volume);

        StartCoroutine(WaitForSongEnd(s.source,SongIdex));
        SongIdex++;
        
        //if (s.isLooping)
        //    s.source.Play();
        //else
        //    s.source.PlayOneShot(s.clip, volume);
    }

    void ReleaseSong(int index)
    {
        Debug.Log("ReleaseSong: " + index.ToString());
        SongClip s = songs[index];
        if (s == null)
            return;
        if (s.source.isPlaying)
            s.source.Stop();
        s.clip.UnloadAudioData();
        //Addressables.Release(s.songAddressable);
        PlaySong(1.0f);
    }
    IEnumerator WaitForSongEnd(AudioSource audioSource, int index)
    {
        yield return new WaitUntil(() => !audioSource.isPlaying && (audioSource.time == 0f));
        Debug.Log("WaitForSongEnd: Music Ended.");
        ReleaseSong(index);
    }
}
