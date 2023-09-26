using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using TMPro;

public class LobbyListItem : MonoBehaviour
{
    public CSteamID lobbySteamId;
    public string lobbyName;
    public string GameMode;
    public int numberOfPlayers;
    public int maxNumberOfPlayers;

    [SerializeField] private TextMeshProUGUI LobbyNameText;
    [SerializeField] private TextMeshProUGUI NumerOfPlayersText;
    [SerializeField] private TextMeshProUGUI _gameModeText;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetLobbyItemValues()
    {
        LobbyNameText.text = lobbyName;
        _gameModeText.text = "| Mode: " + GameMode + " |";
        NumerOfPlayersText.text = "Number of Players: " + numberOfPlayers.ToString() + "/" + maxNumberOfPlayers.ToString();
    }
    public void JoinLobby()
    {
        Debug.Log("JoinLobby: Player selected to join lobby with steam id of: " + lobbySteamId.ToString());
        TitleScreenManager.instance.ResetGameServerSettings();
        if (this.GameMode == "Football")
        {
            GolfSteamLobby.instance.JoiningFishNet = false;
            SteamLobby.instance.JoiningMirror = true;
            SteamLobby.instance.JoinLobby(lobbySteamId);
        }
        else
        {
            GolfSteamLobby.instance.JoiningFishNet = true;
            SteamLobby.instance.JoiningMirror = false;
            GolfSteamLobby.instance.JoinLobby(lobbySteamId);
        }
        
        
    }
}
