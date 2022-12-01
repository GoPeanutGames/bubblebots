using BubbleBots.Modes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeSelectController : MonoBehaviour
{
    public void NethermodeButtonClick()
    {

    }

    public void PlayFreeButtonClick()
    {
        ModeManager.Instance.SetMode(Mode.FREE);
        SceneManager.LoadScene(EnvironmentManager.Instance.GetSceneName());
    }
}
