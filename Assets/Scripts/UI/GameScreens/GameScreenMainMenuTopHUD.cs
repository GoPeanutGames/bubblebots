using System;
using System.Collections.Generic;
using BubbleBots.Server.Player;
using TMPro;
using UnityEngine;

public class GameScreenMainMenuTopHUD : GameScreen
{
    public enum PlayerResource
    {
        Bubbles,
        Gems,
        Energy
    }

    public static Action ResourcesSet;

    public GameObject settingsGroup;
    public GameObject playerInfoGroup;
    public TextMeshProUGUI bubblesText;
    public TextMeshProUGUI gemsText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI usernameText;
    public GameObject bubblesTooltip;
    public GameObject gemsTooltip;
    public GameObject energyTooltip;

    const int MAX_ENERGY = 10;

    private Dictionary<PlayerResource, TextMeshProUGUI> _resourceTextMap;
    private bool _resourcesSet = false;

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

    public void ShowSettingsGroup()
    {
        settingsGroup.SetActive(true);
    }

    public void HideSettingsGroup()
    {
        settingsGroup.SetActive(false);
    }

    public void ShowPlayerInfoGroup()
    {
        playerInfoGroup.SetActive(true);
    }

    public void HidePlayerInfoGroup()
    {
        playerInfoGroup.SetActive(false);
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

    public void SetUsername(string username)
    {
        usernameText.text = username;
    }

    private void DeactivateGameObject(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }
    
    public void SwitchBubblesTooltipActive()
    {
        bubblesTooltip.SetActive(!bubblesTooltip.activeSelf);
        if (bubblesTooltip.activeSelf)
        {
            DeactivateGameObject(energyTooltip);
            DeactivateGameObject(gemsTooltip);
        }
    }
    
    public void SwitchGemsTooltipActive()
    {
        gemsTooltip.SetActive(!gemsTooltip.activeSelf);
        if (gemsTooltip.activeSelf)
        {
            DeactivateGameObject(bubblesTooltip);
            DeactivateGameObject(energyTooltip);
        }
    }
    
    public void SwitchEnergyTooltipActive()
    {
        energyTooltip.SetActive(!energyTooltip.activeSelf);
        if (energyTooltip.activeSelf)
        {
            DeactivateGameObject(bubblesTooltip);
            DeactivateGameObject(gemsTooltip);
        }
    }
    
    public bool AreResourcesSet()
    {
        return _resourcesSet;
    }
}