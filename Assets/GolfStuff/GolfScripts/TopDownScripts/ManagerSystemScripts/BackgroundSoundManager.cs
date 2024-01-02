using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class BackgroundSoundManager : MonoBehaviour
{
    public static BackgroundSoundManager instance;
    public AudioMixer audioMixer;
    public AudioMixerGroup mixerGroup;
    public BackgroundClip[] clips;
    [SerializeField] AudioSource _source;

    private void Awake()
    {
        MakeInstance();
        if (!this._source)
            this._source = GetComponent<AudioSource>();
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

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlaySound(string soundName, float volume)
    {
        Debug.Log("BackgroundSoundManager: PlaySound: " + soundName + ":" + volume.ToString());
        
        BackgroundClip clipToPlay = Array.Find(clips, clip => clip.name == soundName);
        if (clipToPlay == null)
        {
            Debug.Log("BackgroundSoundManager: PlaySound: No clip found");
            return;
        }
        if (_source.clip != null)
        {
            if (clipToPlay.clip.name == _source.clip.name)
            {
                Debug.Log("BackgroundSoundManager: PlaySound: Playing the same clip but with different volumes?" + clipToPlay.clip.name + ":" + volume.ToString() + " " + _source.clip.name + ":" + _source.volume.ToString());
                _source.volume = volume;
                return;
            }
        }
        
        StopSound();

        if (!_source.enabled)
            _source.enabled = true;
        _source.volume = volume;
        clipToPlay.clip.LoadAudioData();

        _source.clip = clipToPlay.clip;

        //_source.PlayOneShot(clipToPlay.clip, volume);
        _source.Play();

    }
    public void StopSound()
    {
        Debug.Log("BackgroundSoundManager: StopSound:");
        if (!_source)
            return;
        if (!_source.isPlaying)
            return;
        if (!_source.clip)
            return;

        _source.clip.UnloadAudioData();

        _source.Stop();
    }
}
