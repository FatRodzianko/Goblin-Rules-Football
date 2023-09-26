using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LobbyMenu : MonoBehaviour
{
    public static LobbyMenu instance;

    [Header("UI Elements")]
    [SerializeField] GameObject _lobbyMenuCanvas;
    [SerializeField] Button _createLobbyButton;
    [SerializeField] Button _joinLobbyButton;
    [SerializeField] TMP_InputField _joinLobbyID;

    private void Awake()
    {
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
        EnableLobbyMenu(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void EnableLobbyMenu(bool enable)
    {
        _lobbyMenuCanvas.SetActive(enable);
    }
    public void CreateLobby()
    {
        Debug.Log("LobbyMenu: CreateLobby: ");
        GolfSteamLobby.instance.JoiningFishNet = true;
        GolfSteamLobby.instance.CreateLobby();
        EnableLobbyMenu(false);
    }
    public void JoinLobby()
    {
        if (string.IsNullOrEmpty(_joinLobbyID.text))
            return;
        ulong id = Convert.ToUInt64(_joinLobbyID.text);
        GolfSteamLobby.instance.JoiningFishNet = true;
        GolfSteamLobby.instance.JoinLobby(new Steamworks.CSteamID(id));
        EnableLobbyMenu(false);
    }
}
