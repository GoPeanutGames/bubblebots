using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginController : MonoBehaviour
{
    public GameObject LoginScreen;
    public GameObject GuestScreen;
    public GameObject LoadingScreen;
    public WalletLoginController walletLoginController;
    public LoginServerPlayerController loginServerPlayerController;

    private void Start()
    {
        SoundManager.Instance.PlayStartMusic();
        TryLoginFromSave();
    }

    private void TryLoginFromSave()
    {
        string address = UserManager.Instance.GetPlayerWalletAddress();
        if(string.IsNullOrEmpty(address) == false)
        {
            StartLogin(address);
        }
    }

    private void StartLogin(string address)
    {
        AnalyticsManager.Instance.InitAnalyticsWithWallet(address);
        loginServerPlayerController.GetOrCreatePlayer(address);
        UserManager.PlayerType = PlayerType.LoggedInUser;
        LoginScreen.SetActive(false);
        LoadingScreen.SetActive(true);
    }

    public void LoginWithMetamask()
    {
        walletLoginController.LoginWithMetamask();
    }

    public void FirstPlayAsGuest()
    {
        GuestScreen.SetActive(true);
        LoginScreen.SetActive(false);
    }

    public void SecondPlayAsGuest()
    {
        UserManager.PlayerType = PlayerType.Guest;
        AnalyticsManager.Instance.InitAnalyticsGuest();
        SceneManager.LoadScene(EnvironmentManager.Instance.GetSceneName());
    }

    public void InitSession(string address)
    {
        StartLogin(address);
    }
}
