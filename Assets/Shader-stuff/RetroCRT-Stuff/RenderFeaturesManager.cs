using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RenderFeaturesManager : MonoBehaviour
{
    public static RenderFeaturesManager instance;
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

        // The presenter is the only thing that draws the effect. With none in the scene yet, the
        // setting is only remembered: each presenter applies it once its rig is built.
        if (CRTPresenter.Instance != null)
            CRTPresenter.Instance.SetEffectEnabled(enable);
    }
}
