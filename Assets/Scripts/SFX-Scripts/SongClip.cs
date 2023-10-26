using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class SongClip
{
    public string name;
    public AudioClip clip;
    public string ClipAddress;
    [Range(0f, 1f)]
    public float volume;
    [Range(0.1f, 3f)]
    public float pitch;
    public bool isLooping;
    public AsyncOperationHandle<AudioClip> songAddressable;

    public AudioSource source;

}
