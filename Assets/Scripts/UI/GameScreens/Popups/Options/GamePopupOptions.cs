using System.Collections;
using System.Collections.Generic;
using BubbleBots.User;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePopupOptions : GameScreen
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
    public Button changeNameButton;
    public GameObject syncProgressButton;
    public GameObject signOutButton;

    private AvatarInformation currentAvatar;
    
    private void Start()
    {
        InitialiseMusicToggle();
        InitialiseHintsToggle();
        SetPlayerAvatar(UserManager.Instance.GetPlayerAvatar());
        RefreshPlayerUsername();
        RefreshAuthState();
    }

    private void InitialiseMusicToggle()
    {
        bool musicOn = UserManager.Instance.GetPlayerSettings().music;
        MusicToggle.SetIsOnWithoutNotify(musicOn);
        MusicToggleOff.SetActive(!musicOn);
        MusicToggleOn.SetActive(musicOn);
    }

    private void InitialiseHintsToggle()
    {
        bool hinting = UserManager.Instance.GetPlayerSettings().hints;
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

    public void RefreshAuthState()
    {
        changeNameButton.interactable = UserManager.PlayerType == PlayerType.LoggedInUser; 
        signOutButton.SetActive(UserManager.PlayerType == PlayerType.LoggedInUser);
        syncProgressButton.SetActive(UserManager.PlayerType == PlayerType.Guest);
    }
}