using Steamworks;
using UnityEngine;
using UnityEngine.UI;

// Shows a Steam avatar for whoever is being displayed. Steam may not be running at all (solo play, a dev
// session), and an avatar Steam has not cached yet arrives later through a callback, so the id that was
// asked for is kept to check the callback against.
public class PlayerAvatarDisplay : MonoBehaviour
{
    [SerializeField] private RawImage _avatarImage;
    [SerializeField] private Texture _fallbackAvatar;

    private ulong _steamIdShowing;
    private Callback<AvatarImageLoaded_t> _avatarImageLoaded;

    private void Awake()
    {
        if (!_avatarImage)
            _avatarImage = GetComponent<RawImage>();
    }
    private void OnEnable()
    {
        if (SteamManager.Initialized)
            _avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
    }
    private void OnDisable()
    {
        if (_avatarImageLoaded == null)
            return;

        _avatarImageLoaded.Dispose();
        _avatarImageLoaded = null;
    }
    public void ShowAvatarFor(ulong steamId)
    {
        _steamIdShowing = steamId;

        // Every Steamworks call throws when Steam is not running, and a solo session has no Steam id.
        if (!SteamManager.Initialized || steamId == 0)
        {
            ShowFallback();
            return;
        }

        int imageId = SteamFriends.GetLargeFriendAvatar((CSteamID)steamId);

        // Not in Steam's cache. It downloads the image and OnAvatarImageLoaded picks it up.
        if (imageId == -1)
        {
            ShowFallback();
            return;
        }

        ApplyImage(imageId);
    }
    void ShowFallback()
    {
        if (!_avatarImage)
            return;

        _avatarImage.texture = _fallbackAvatar;

        // A RawImage with no texture draws a white box, so hide it instead when there is nothing to show.
        _avatarImage.enabled = _fallbackAvatar != null;
    }
    void ApplyImage(int imageId)
    {
        if (!_avatarImage)
            return;

        Texture2D avatar = SteamImageAsTexture(imageId);
        if (avatar == null)
        {
            ShowFallback();
            return;
        }

        _avatarImage.texture = avatar;
        _avatarImage.enabled = true;
    }
    void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        // The turn may have moved on to someone else while Steam was downloading this one.
        if (callback.m_steamID.m_SteamID != _steamIdShowing)
            return;

        ApplyImage(callback.m_iImage);
    }
    static Texture2D SteamImageAsTexture(int imageId)
    {
        if (!SteamUtils.GetImageSize(imageId, out uint width, out uint height))
            return null;

        byte[] image = new byte[width * height * 4];
        if (!SteamUtils.GetImageRGBA(imageId, image, (int)(width * height * 4)))
            return null;

        // Steam hands the rows over top to bottom and LoadRawTextureData wants them bottom to top, so
        // without reversing them the avatar comes out upside down.
        int stride = (int)width * 4;
        byte[] rightWayUp = new byte[image.Length];
        for (int row = 0; row < (int)height; row++)
        {
            System.Array.Copy(image, row * stride, rightWayUp, ((int)height - 1 - row) * stride, stride);
        }

        Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
        texture.LoadRawTextureData(rightWayUp);
        texture.Apply();
        return texture;
    }
}
