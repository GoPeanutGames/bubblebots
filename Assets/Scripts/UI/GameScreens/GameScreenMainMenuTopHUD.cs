using UnityEngine;

public class GameScreenMainMenuTopHUD : GameScreen
{
    public GameObject settingsGroup;
    public GameObject playerInfoGroup;


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

}
