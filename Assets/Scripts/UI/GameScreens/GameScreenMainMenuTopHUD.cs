using System.Collections.Generic;
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

    public GameObject settingsGroup;
    public GameObject playerInfoGroup;
    public TextMeshProUGUI BubblesText;
    public TextMeshProUGUI GemsText;
    public TextMeshProUGUI EnergyText;

    const int MAX_ENERGY = 10;

    private Dictionary<PlayerResource, TextMeshProUGUI> _resourceTextMap;

    private void Start()
    {
        _resourceTextMap = new()
        {
            { PlayerResource.Bubbles, BubblesText },
            { PlayerResource.Energy, EnergyText },
            { PlayerResource.Gems, GemsText }
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
}