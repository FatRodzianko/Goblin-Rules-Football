using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System;

public class PlayerListItem : MonoBehaviour
{

    public string PlayerName;
    public int ConnectionId;
    public bool isPlayerReady;
    //public ulong playerSteamId;
    //private bool avatarRetrieved;
    public bool isTeamGrey;

    [SerializeField] private TextMeshProUGUI PlayerNameText;
    [SerializeField] private TextMeshProUGUI PlayerReadyStatus;
    [SerializeField] private RawImage playerAvatar;
    [SerializeField] Color readyColor;
    [SerializeField] Color notReadyColor;
    [SerializeField] private GameObject GoblinTypeDropdown;
    [SerializeField] private TextMeshProUGUI goblinSelected;

    public GameObject localLobbyPlayerObject;
    public LobbyPlayer localLobbyPlayerScript;

    private bool isLocalPlayerFoundYet = false;

    //protected Callback<AvatarImageLoaded_t> avatarImageLoaded;
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
        /*if (!avatarRetrieved)
            GetPlayerAvatar();*/
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
    /*void GetPlayerAvatar()
    {
        int imageId = SteamFriends.GetLargeFriendAvatar((CSteamID)playerSteamId);

        if (imageId == -1)
        {
            Debug.Log("GetPlayerAvatar: Avatar not in cache. Will need to download from steam.");
            return;
        }

        playerAvatar.texture = GetSteamImageAsTexture(imageId);
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
        }
        else
        {
            return;
        }
    }*/
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
}
