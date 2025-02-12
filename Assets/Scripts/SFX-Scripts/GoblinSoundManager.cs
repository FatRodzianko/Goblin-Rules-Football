using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class GoblinSoundManager : MonoBehaviour
{
    [SerializeField] GoblinScript myGoblin;

    public AudioMixer audioMixer;
    public AudioMixerGroup mixerGroup;
    public Sound[] sounds;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.outputAudioMixerGroup = mixerGroup;
            //s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.playOnAwake = false;
            s.source.loop = s.isLooping;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlaySound(string name, float volume)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
            return;
        Debug.Log("GoblinSoundManager PlaySound: " + name);
        /*if (!s.source.enabled)
            s.source.enabled = true;*/
        s.source.volume = volume;
        if (s.isLooping)
            s.source.Play();
        else
            s.source.PlayOneShot(s.clip, volume);
    }
    public void StopSound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
            return;
        Debug.Log("GoblinSoundManager StopSound: " + name + " ");
        //s.source.mute = true;
        s.source.Stop();

        Debug.Log("StopSound: " + name + " is still playing? " + s.source.isPlaying.ToString());
    }
}
