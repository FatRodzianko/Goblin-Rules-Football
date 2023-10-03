using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using TMPro;

public class GolfPlayerListItem : MonoBehaviour
{
    public string PlayerName;
    public int ConnectionId;
    public bool isPlayerReady;
    public ulong playerSteamId;
    private bool avatarRetrieved;

    [SerializeField] private TextMeshProUGUI PlayerNameText;
    [SerializeField] private TextMeshProUGUI PlayerReadyStatus;
    [SerializeField] private RawImage playerAvatar;
    [SerializeField] Color readyColor;
    [SerializeField] Color notReadyColor;
    [SerializeField] private Image _ballColorIcon;

    public GameObject localNetworkPlayerObject;
    public NetworkPlayer localNetworkPlayerScript;

    private bool isLocalPlayerFoundYet = false;

    protected Callback<AvatarImageLoaded_t> avatarImageLoaded;

    // Start is called before the first frame update
    void Start()
    {
        avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdatePlayerItemReadyStatus(bool playerReady)
    {
        isPlayerReady = playerReady;
        if (playerReady)
        {
            PlayerReadyStatus.text = "Ready";
            PlayerReadyStatus.color = readyColor;
        }
        else
        {
            PlayerReadyStatus.text = "Not Ready";
            PlayerReadyStatus.color = notReadyColor;
        }
    }
    public void GetPlayerAvatar()
    {
        Debug.Log("GetPlayerAvatar: with player with steam id of: " + playerSteamId.ToString());
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
    }
    public void UpdateColorIcon(Color newColor)
    {
        _ballColorIcon.color = newColor;
    }
    public void UpdatePlayerName(string playerName)
    {
        PlayerName = playerName;
        PlayerNameText.text = playerName;
    }
}
