using System;
using System.Collections;
using System.Collections.Generic;
using BubbleBots.Server.Player;
using BubbleBots.User;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameScreenHomeHeader : GameScreenAnimatedShowHide
{
    public enum PlayerResource
    {
        Bubbles,
        Gems,
        Energy
    }

    public static Action ResourcesSet;

    public TextMeshProUGUI bubblesText;
    public TextMeshProUGUI gemsText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI usernameText;
    public Image avatarImage;

    const int MAX_ENERGY = 10;

    private Dictionary<PlayerResource, TextMeshProUGUI> _resourceTextMap;
    private bool _resourcesSet = false;

    private AvatarInformation currentAvatar;
    
    private void Start()
    {
        _resourceTextMap = new()
        {
            { PlayerResource.Bubbles, bubblesText },
            { PlayerResource.Energy, energyText },
            { PlayerResource.Gems, gemsText }
        };
        UserManager.Instance.GetPlayerResources();
        UserManager.CallbackWithResources += SetResources;
    }

    private void OnDestroy()
    {
        UserManager.CallbackWithResources -= SetResources;
    }

    private void SetResources(GetPlayerWallet wallet)
    {
        SetTopInfo(PlayerResource.Bubbles, wallet.bubbles);
        SetTopInfo(PlayerResource.Energy, wallet.energy);
        SetTopInfo(PlayerResource.Gems, wallet.gems);
        ResourcesSet?.Invoke();
    }

    private void SetTopInfo(PlayerResource resource, int value)
    {
        _resourcesSet = true;
        if (resource == PlayerResource.Energy)
        {
            _resourceTextMap[resource].text = value.ToString() + "/" + MAX_ENERGY;
        }
        else
        {
            _resourceTextMap[resource].text = value.ToString();
        }
    }
    
    private IEnumerator CheckIfNftAvailable()
    {
        bool waiting = true;
        while (waiting)
        {
            NFTImage selectedImage = new List<NFTImage>(UserManager.Instance.NftManager.GetAvailableNfts()).Find((image)=> image.tokenId == currentAvatar.id);
            if (selectedImage is { loaded: true })
            {
                avatarImage.sprite = selectedImage.sprite;
                waiting = false;
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    public bool AreResourcesSet()
    {
        return _resourcesSet;
    }

    public void RefreshData()
    {
        currentAvatar = UserManager.Instance.GetPlayerAvatar();
        StopCoroutine(CheckIfNftAvailable());
        if (currentAvatar.isNft)
        {
            StartCoroutine(CheckIfNftAvailable());
        }
        else
        {
            avatarImage.sprite = UserManager.Instance.PlayerAvatars[currentAvatar.id];
        }
        usernameText.text = UserManager.Instance.GetPlayerUserName();
    }
}