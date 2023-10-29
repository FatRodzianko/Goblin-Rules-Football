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
}
