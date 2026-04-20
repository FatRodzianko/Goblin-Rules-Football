using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NoiseLevels
{
    None,
    Low,
    Medium,
    High
}
[Serializable]
public class NoiseThresholdMapping
{
    public NoiseLevels NoiseLevel;
    public int VolumeThreshold;
}
[Serializable]
public class NoiseLevelUISpriteMapping
{
    public NoiseLevels NoiseLevel;
    public Sprite Sprite;
}

[CreateAssetMenu(fileName = "New Noise UI Mapping", menuName = "BombRun/UI/New Scriptable Noise UI Mapping")]
public class ScriptableNoiseUIMapping : ScriptableObject
{
    [SerializeField] private List<NoiseThresholdMapping> _noiseThresholdMappings = new List<NoiseThresholdMapping>();
    [SerializeField] private List<NoiseLevelUISpriteMapping> _noiseLevelUISpriteMappings = new List<NoiseLevelUISpriteMapping>();

    public List<NoiseThresholdMapping> NoiseThresholdMapping()
    {
        return _noiseThresholdMappings;
    }
    public List<NoiseLevelUISpriteMapping> NoiseLevelUISpriteMapping()
    {
        return _noiseLevelUISpriteMappings;
    }
    public Sprite GetSpriteFromNoiseVolume(int noiseVolume)
    {
        NoiseLevels highestNoiseLevel = NoiseLevels.None;

        foreach (NoiseThresholdMapping noiseThresholdMapping in _noiseThresholdMappings)
        {
            if (noiseVolume >= noiseThresholdMapping.VolumeThreshold)
            {
                highestNoiseLevel = noiseThresholdMapping.NoiseLevel;
            }
        }

        foreach (NoiseLevelUISpriteMapping noiseLevelUISpriteMapping in _noiseLevelUISpriteMappings)
        {
            if (highestNoiseLevel == noiseLevelUISpriteMapping.NoiseLevel)
            {
                return noiseLevelUISpriteMapping.Sprite;
            }
        }
        return null;
    }
}
