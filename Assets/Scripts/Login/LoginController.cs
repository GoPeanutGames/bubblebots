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
        TryLoginFromSave();
        SoundManager.Instance.PlayStartMusic();
    }

    private void TryLoginFromSave()
    {
        string address = LeaderboardManager.Instance.PlayerWalletAddress;
        if(string.IsNullOrEmpty(address) == false)
        {
            StartLogin(address);
        }
    }

    private void StartLogin(string address)
    {
        AnalyticsManager.Instance.InitAnalyticsWithWallet(address);
        loginServerPlayerController.CreatePlayer(address);
        LoginScreen.SetActive(false);
        LoadingScreen.SetActive(true);
        LeaderboardManager.Instance.SetPlayerWalletAddress(address);
        LeaderboardManager.Instance.CurrentPlayerType = PlayerType.LoggedInUser;
        LeaderboardManager.Instance.GetPlayerScore((res) =>
        {
            LoadingScreen.SetActive(false);
            SceneManager.LoadScene(EnvironmentManager.Instance.GetSceneName());
        });
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
        LeaderboardManager.Instance.SetGuestMode();
        LeaderboardManager.Instance.CurrentPlayerType = PlayerType.Guest;
        AnalyticsManager.Instance.InitAnalyticsGuest();
        SceneManager.LoadScene(EnvironmentManager.Instance.GetSceneName());
    }

    public void InitSession(string address)
    {
        StartLogin(address);
    }
}
