using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class GameScreenMainMenuTopHUD : GameScreen
{
    public enum PlayerResource
    {
        Bubbles,
        Gems,
        Energy
    }

    public GameObject settingsGroup;
    public GameObject playerInfoGroup;
    public TextMeshProUGUI bubblesText;
    public TextMeshProUGUI gemsText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI usernameText;

    const int MAX_ENERGY = 10;

    private Dictionary<PlayerResource, TextMeshProUGUI> _resourceTextMap;

    private void Start()
    {
        _resourceTextMap = new()
        {
            { PlayerResource.Bubbles, bubblesText },
            { PlayerResource.Energy, energyText },
            { PlayerResource.Gems, gemsText }
        };
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

    public void SetTopInfo(PlayerResource resource, int value)
    {
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
}