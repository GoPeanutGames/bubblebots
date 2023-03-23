using UnityEngine;

public class GameScreenLogin : GameScreen
{
    public GameObject loadingOverlay;


    public void ShowLoading()
    {
        loadingOverlay.SetActive(true);
    }

}