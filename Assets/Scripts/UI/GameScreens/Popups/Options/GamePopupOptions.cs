using System.Collections;
using System.Collections.Generic;
using BubbleBots.User;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePopupOptions : GameScreenAnimatedEntryExit
{
    [Header("Music")]
    public Toggle MusicToggle;
    public GameObject MusicToggleOff;
    public GameObject MusicToggleOn;
    
    [Header("Hints")] 
    public Toggle HintsToggle;
    public GameObject HintsToggleOff;
    public GameObject HintsToggleOn;

    [Header("Other")]
    public Image playerAvatar;
    public TextMeshProUGUI playerUsername;

    private AvatarInformation currentAvatar;
    
    private void Start()
    {
        InitialiseMusicToggle();
        InitialiseHintsToggle();
        SetPlayerAvatar(UserManager.Instance.GetPlayerAvatar());
        RefreshPlayerUsername();
    }

    private void InitialiseMusicToggle()
    {
        bool muted = SoundManager.Instance.IsMuted();
        MusicToggle.SetIsOnWithoutNotify(!muted);
        MusicToggleOff.SetActive(muted);
        MusicToggleOn.SetActive(!muted);
    }

    private void InitialiseHintsToggle()
    {
        bool hinting = UserManager.Instance.GetPlayerHints();
        HintsToggle.SetIsOnWithoutNotify(hinting);
        HintsToggleOff.SetActive(!hinting);
        HintsToggleOn.SetActive(hinting);
    }

    private IEnumerator CheckIfNftAvailable()
    {
        bool waiting = true;
        int id = currentAvatar.id;
        while (waiting)
        {
            NFTImage selectedImage = new List<NFTImage>(UserManager.Instance.NftManager.GetAvailableNfts()).Find((image)=> image.tokenId == id);
            if (selectedImage.loaded)
            {
                playerAvatar.sprite = selectedImage.sprite;
                waiting = false;
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    public void MusicToggleValueChanged(bool value)
    {
        MusicToggleOff.SetActive(!value);
        MusicToggleOn.SetActive(value);
    }

    public void HintsToggleValueChanged(bool value)
    {
        HintsToggleOff.SetActive(!value);
        HintsToggleOn.SetActive(value);
    }

    public bool GetFinalMusicValue()
    {
        return MusicToggle.isOn;
    }

    public bool GetFinalHintsValue()
    {
        return HintsToggle.isOn;
    }

    public void SetPlayerAvatar(AvatarInformation avatar)
    {
        currentAvatar = avatar;
        StopCoroutine(CheckIfNftAvailable());
        if (avatar.isNft)
        {
            StartCoroutine(CheckIfNftAvailable());
        }
        else
        {
            playerAvatar.sprite = UserManager.Instance.PlayerAvatars[avatar.id];
        }
        Debug.LogWarning("Switching to: " + currentAvatar.id);
    }

    public void RefreshPlayerUsername()
    {
        playerUsername.text = UserManager.Instance.GetPlayerUserName();
    }
}