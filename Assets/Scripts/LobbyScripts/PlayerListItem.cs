using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Steamworks;

public class PlayerListItem : MonoBehaviour
{

    public string PlayerName;
    public int ConnectionId;
    // Which LobbyPlayer this row is for. ConnectionId cannot identify a row once bots exist: every bot has -1,
    // so matching on it gave the first bot a row and silently treated every later bot as already present.
    public int lobbyPlayerObjectId = -1;
    public bool isBot;
    public bool isPlayerReady;
    public ulong playerSteamId;
    private bool avatarRetrieved;
    public bool isTeamGrey;

    [SerializeField] private TextMeshProUGUI PlayerNameText;
    [SerializeField] private TextMeshProUGUI PlayerReadyStatus;
    [SerializeField] private RawImage playerAvatar;
    // Shown in the avatar slot for a bot, which has no Steam account to fetch one from. playerAvatar is a RawImage, so
    // this is the sprite's texture rather than the sprite.
    [SerializeField] private Texture botAvatar;
    [SerializeField] Color readyColor;
    [SerializeField] Color notReadyColor;
    [SerializeField] private GameObject GoblinTypeDropdown;
    // Sits where GoblinTypeDropdown does, since a bot has no goblin to pick in the lobby. Only the host sees it.
    [SerializeField] private GameObject KickBotButton;
    [SerializeField] private TextMeshProUGUI goblinSelected;

    [Header("DLC Image Stuff")]
    [SerializeField] private Image dlcImage;
    private bool dlcImageRetrieved;
    [SerializeField] Sprite dlc99;
    [SerializeField] Sprite dlc199;
    [SerializeField] Sprite dlcBoth;
    [SerializeField] Sprite noDLC;

    public GameObject localLobbyPlayerObject;
    public LobbyPlayer localLobbyPlayerScript;

    private bool isLocalPlayerFoundYet = false;

    protected Callback<AvatarImageLoaded_t> avatarImageLoaded;
    // Start is called before the first frame update
    void Start()
    {
        FindLocalLobbyPlayer();
        IsThisForLocalLobbyPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetPlayerListItemValues()
    {
        PlayerNameText.text = PlayerName;
        UpdatePlayerItemReadyStatus();
        if (isBot)
        {
            ApplyBotLook();
            return;
        }
        // Say it rather than relying on the prefab being saved with it off: a player's row never has a kick button.
        if (KickBotButton != null)
            KickBotButton.SetActive(false);
        if (!avatarRetrieved)
            GetPlayerAvatar();
        GetDLCImageInfoFromLobbyPlayer();
    }
    // A bot has no Steam account, so no avatar to fetch and no DLC to own, and no goblin to pick in the lobby: it takes
    // whatever roles are left at Start Game. So the icon stands in for the avatar, the DLC badge goes, and the kick
    // button takes the goblin dropdown's place - for the host only, since only the host can remove it.
    void ApplyBotLook()
    {
        if (playerAvatar != null)
        {
            playerAvatar.texture = botAvatar;
            playerAvatar.enabled = botAvatar != null;
            // The prefab flips the avatar vertically because Steam hands its images back upside down. The bot icon is
            // an ordinary sprite, so it has to be flipped back the right way up.
            SetAvatarUpsideDown(false);
        }
        if (dlcImage != null)
            dlcImage.enabled = false;
        if (GoblinTypeDropdown != null)
            GoblinTypeDropdown.SetActive(false);
        if (goblinSelected != null)
            goblinSelected.gameObject.SetActive(false);
        if (KickBotButton != null)
            KickBotButton.SetActive(IsLocalPlayerTheHost());
    }
    // Steam's avatar images arrive upside down, which the prefab corrects with a Y scale of -1. Anything that is not a
    // Steam image, like the bot icon, needs that undone.
    void SetAvatarUpsideDown(bool upsideDown)
    {
        if (playerAvatar == null)
            return;
        Vector3 scale = playerAvatar.transform.localScale;
        scale.y = upsideDown ? -Mathf.Abs(scale.y) : Mathf.Abs(scale.y);
        playerAvatar.transform.localScale = scale;
    }
    bool IsLocalPlayerTheHost()
    {
        GameObject localPlayerObject = GameObject.Find("LocalLobbyPlayer");
        if (localPlayerObject == null)
            return false;
        LobbyPlayer localPlayer = localPlayerObject.GetComponent<LobbyPlayer>();
        return localPlayer != null && localPlayer.IsGameLeader;
    }
    // Wired to KickBotButton. Only the host has the button, and the host is the server, so this can remove the bot
    // directly instead of going through an Rpc.
    public void KickBotButtonPressed()
    {
        if (!isBot)
            return;
        LobbyPlayer bot;
        if (!ObjectLookup.TryGet(lobbyPlayerObjectId, out bot))
        {
            Debug.Log("KickBotButtonPressed: could not find the bot for row " + lobbyPlayerObjectId.ToString());
            return;
        }
        NetworkManagerGRF.singleton.RemoveBot(bot);
    }
    public void UpdatePlayerItemReadyStatus()
    {
        if (isPlayerReady)
        {
            PlayerReadyStatus.text = "READY";
            PlayerReadyStatus.color = Color.green;
        }
        else
        {
            PlayerReadyStatus.text = "NOT READY";
            PlayerReadyStatus.color = Color.red;
        }
    }
    public void FindLocalLobbyPlayer()
    {
        localLobbyPlayerObject = GameObject.Find("LocalLobbyPlayer");
        localLobbyPlayerScript = localLobbyPlayerObject.GetComponent<LobbyPlayer>();
        isLocalPlayerFoundYet = true;
    }
    public void IsThisForLocalLobbyPlayer()
    {
        if (this.PlayerName == localLobbyPlayerScript.PlayerName && this.ConnectionId == localLobbyPlayerScript.ConnectionId)
        {
            if (localLobbyPlayerScript.myPlayerListItem == null)
                localLobbyPlayerScript.myPlayerListItem = this;
            ActivateGoblinDropdown();
        }
        else
        {
            if (!LobbyManager.instance.is1v1)
            {
                GoblinTypeDropdown.SetActive(false);
                //goblinSelected.gameObject.SetActive(false);
                //Debug.Log("ActivateGoblinSelectedText: IsThisForLocalLobbyPlayer");
            }
                
        }
    }
    public void ActivateGoblinDropdown()
    {
        if (!LobbyManager.instance.is1v1)
        {
            if (localLobbyPlayerScript.isGoblinSelected)
            {
                GoblinTypeDropdown.SetActive(false);
                goblinSelected.gameObject.SetActive(true);
            }
            else
            {
                GoblinTypeDropdown.SetActive(true);
                goblinSelected.gameObject.SetActive(false);
            }
        }
    }
    public void SelectGoblinButton()
    {
        if (this.PlayerName == localLobbyPlayerScript.PlayerName && this.ConnectionId == localLobbyPlayerScript.ConnectionId)
        {
            int dropDownIndex = GoblinTypeDropdown.GetComponent<TMP_Dropdown>().value;
            string goblinSelected = GoblinTypeDropdown.GetComponent<TMP_Dropdown>().options[dropDownIndex].text;
            Debug.Log("SelectGoblinButton: Index of dropdown: " + dropDownIndex.ToString() + " and the text at that index: " + goblinSelected);
            localLobbyPlayerScript.SelectGoblin(goblinSelected);
        }
    }
    public void SetGoblinSelectedText(string selectedGoblinText)
    {
        Debug.Log("SetGoblinSelectedText: for playerlistitem for player: " + this.PlayerName + " set the goblin text to: " + selectedGoblinText);
        try
        {
            goblinSelected.text = selectedGoblinText;
        }
        catch (Exception e)
        {
            Debug.Log("SetGoblinSelectedText: Error: " + e);
        }
        
    }
    public void ActivateGoblinSelectedText(bool enable)
    {
        Debug.Log("ActivateGoblinSelectedText: for player: " + this.PlayerName + " enable goblin selected text? " + enable.ToString());
        try
        {
            goblinSelected.gameObject.SetActive(enable);
        }
        catch (Exception e)
        {
            Debug.Log("ActivateGoblinSelectedText: Error: " + e);
        }
        
    }
    public void UpdateGoblinsAvailable(List<string> goblinsAvailable)
    {
        Debug.Log("UpdateGoblinsAvailable: Number in list: " + goblinsAvailable.Count.ToString());
        GoblinTypeDropdown.GetComponent<TMP_Dropdown>().ClearOptions();
        GoblinTypeDropdown.GetComponent<TMP_Dropdown>().AddOptions(goblinsAvailable);
    }
    void GetPlayerAvatar()
    {
        // GetLargeFriendAvatar throws when Steam isn't running. This runs from LobbyManager.UpdateUI, which
        // LobbyPlayer.OnStartClient calls, and an exception there aborts the rest of FishNet's start callbacks. A dev
        // session has no Steam ids anyway, so there is no avatar to fetch.
        if (!SteamManager.Initialized)
            return;
        int imageId = SteamFriends.GetLargeFriendAvatar((CSteamID)playerSteamId);

        if (imageId == -1)
        {
            Debug.Log("GetPlayerAvatar: Avatar not in cache. Will need to download from steam.");
            return;
        }

        playerAvatar.texture = GetSteamImageAsTexture(imageId);
        SetAvatarUpsideDown(true);
    }
    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Debug.Log("Executing GetSteamImageAsTexture for player: " + this.PlayerName);
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (isValid)
        {
            Debug.Log("GetSteamImageAsTexture: Image size is valid?");
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                Debug.Log("GetSteamImageAsTexture: Image size is valid for GetImageRBGA?");
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        avatarRetrieved = true;
        return texture;
    }
    private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID == playerSteamId)
        {
            Debug.Log("OnAvatarImageLoaded: Avatar downloaded from steam.");
            playerAvatar.texture = GetSteamImageAsTexture(callback.m_iImage);
            SetAvatarUpsideDown(true);
        }
        else
        {
            return;
        }
    }
    public void SetPlayerTeam(bool isGrey)
    {
        Debug.Log("SetPlayerTeam: To Grey? " + isGrey.ToString() + " for player: " + this.PlayerName + " " + this.ConnectionId);
        if (this.isTeamGrey != isGrey)
        {
            this.isTeamGrey = isGrey;
            if (isLocalPlayerFoundYet)
            {
                if(this.ConnectionId == localLobbyPlayerScript.ConnectionId)
                    localLobbyPlayerScript.UpdateTeam(isGrey);
            }
            else
            {
                try
                {
                    LobbyPlayer playerScript = GameObject.Find("LocalLobbyPlayer").GetComponent<LobbyPlayer>();
                    if (playerScript.ConnectionId == this.ConnectionId)
                        playerScript.UpdateTeam(isGrey);
                }
                catch (Exception e)
                {
                    Debug.Log("SetPlayerTeam: " + e);
                }
            }
        }
    }
    public void UpdateDLCImages(string whatDLCsAreInstalled)
    {
        Debug.Log("UpdateDLCImages: for player with Steam Id of : " + this.playerSteamId.ToString() + " what dlcs are installed: " + whatDLCsAreInstalled);
        try
        {
            if (string.IsNullOrWhiteSpace(whatDLCsAreInstalled))
            {
                dlcImage.sprite = noDLC;
            }
            if (whatDLCsAreInstalled == "none")
            {
                dlcImage.sprite = noDLC;
            }
            if (whatDLCsAreInstalled == "99")
            {
                dlcImage.sprite = dlc99;
            }
            if (whatDLCsAreInstalled == "1_99")
            {
                dlcImage.sprite = dlc199;
            }
            if (whatDLCsAreInstalled == "both")
            {
                dlcImage.sprite = dlcBoth;
            }
        }
        catch (Exception e)
        {
            Debug.Log("UpdateDLCImages: Could not set the dlcImage sprite. Error: " + e);
        }
    }
    public void GetDLCImageInfoFromLobbyPlayer()
    {
        GameObject[] LobbyPlayers = GameObject.FindGameObjectsWithTag("LobbyPlayer");
        if (LobbyPlayers.Length > 0)
        {
            //Debug.Log("GetDLCImageInfoFromLobbyPlayer: Length of lobby players: " + LobbyPlayers.Length.ToString());
            for (int i = 0; i < LobbyPlayers.Length; i++)
            {
                LobbyPlayer lobbyPlayerScript = LobbyPlayers[i].GetComponent<LobbyPlayer>();
                if (this.playerSteamId == lobbyPlayerScript.playerSteamId)
                {
                    //Debug.Log("GetDLCImageInfoFromLobbyPlayer: Matching steam id found. Lobby Player Steam id: " + lobbyPlayerScript.playerSteamId.ToString() + " this player list item steam id: " + this.playerSteamId);
                    this.UpdateDLCImages(lobbyPlayerScript.whatDLCsAreInstalled);
                    break;
                }
            }
        }
    }
}
