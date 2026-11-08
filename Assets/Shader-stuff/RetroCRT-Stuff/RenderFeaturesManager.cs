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
    /// <summary>Whether the player has the CRT effect switched on, for scenes that load later.</summary>
    public bool RetroCRTEnabled { get; private set; }

    public void EnableRetroCRT(bool enable)
    {
        RetroCRTEnabled = enable;
        _globalVolume.enabled = enable;

        // Where a presenter is in the scene it draws the effect, and the renderer feature has to
        // stay off or the effect would be applied twice.
        if (CRTPresenter.Instance != null)
        {
            feature.SetActive(false);
            CRTPresenter.Instance.SetEffectEnabled(enable);
            return;
        }

        feature.SetActive(enable);
    }
}
