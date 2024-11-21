using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }

    [SerializeField] private CinemachineImpulseSource _cinemachineImpulseSource;

    private void Awake()
    {
        MakeInstance();
        _cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();

    }
    void MakeInstance()
    {
        if (Instance != null)
        {
            Debug.Log("MakeInstance: more than one ScreenShake. Destroying...");
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
    }
    public void Shake(float intensity = 1f)
    {
        _cinemachineImpulseSource.GenerateImpulse(intensity);
    }
}
