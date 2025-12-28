using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RenderFeaturesManager : MonoBehaviour
{
    public static RenderFeaturesManager instance;
    [SerializeField] ScriptableRendererFeature feature;
    [SerializeField] Volume _globalVolume;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        MakeInstance();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
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
    public void EnableRetroCRT(bool enable)
    {
        Debug.Log("EnableRetroCRT: " + enable.ToString());
        feature.SetActive(enable);
        _globalVolume.enabled = enable;
    }
}
