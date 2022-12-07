using WalletConnectSharp.Unity;
using UnityEngine;
using System.Runtime.InteropServices;
using Beebyte.Obfuscator;
using BubbleBots.Server.Player;

public class GameStateLogin : GameState
{
    private GameScreenLogin gameScreenLogin;
    private string tempAddress;
    
    public override string GetGameStateName()
    {
        return "game state login";
    }

    [DllImport("__Internal")]
    private static extern void Login();

    public override void Enable()
    {
        gameScreenLogin = Screens.Instance.PushScreen<GameScreenLogin>();
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
        WalletConnect.Instance.NewSessionConnected.AddListener(OnNewWalletSessionConnectedEventFromPlugin);
    }

    private void OnGameEvent(string ev, object context)
    {
        if (ev == GameEvents.ButtonTap)
        {
            OnButtonTap(ev, context);
        }
    }

    private void OnButtonTap(string ev, object context)
    {
        CustomButtonData customButtonData = (CustomButtonData)context;
        switch (customButtonData.buttonId)
        {
            case ButtonId.LoginGuest:
                gameScreenLogin.OnPlayAsGuestPressed();
                break;
            case ButtonId.LoginGuestPlay:
                PlayAsGuest();
                break;
            case ButtonId.LoginMetamask:
                LoginWithMetamask();
                break;

        }
    }

    private void PlayAsGuest()
    {
        UserManager.PlayerType = PlayerType.Guest;
        AnalyticsManager.Instance.InitAnalyticsGuest();
        stateMachine.PushState(new GameStateFreeToPlay());
    }

    private void GoToMainMenu()
    {
        //stateMachine.PushState(new GameStateMainMenu());
        //SceneManager.LoadScene(EnvironmentManager.Instance.GetSceneName());
    }

    private void LoginWithMetamask()
    {
        if (Application.isMobilePlatform == false)
        {
            Login();
        }
        else
        {
            Application.OpenURL(WalletConnect.Instance.ConnectURL);
        }
    }

    [SkipRename]
    public void MetamaskLoginSuccess(string address)
    {
        SoundManager.Instance.PlayMetamaskSfx();
        AnalyticsManager.Instance.InitAnalyticsWithWallet(address);
        GetOrCreatePlayer(address);
        UserManager.PlayerType = PlayerType.LoggedInUser;
        gameScreenLogin.ShowLoadingScreen();
        gameScreenLogin.HideLoginScreen();
    }

    public void OnNewWalletSessionConnectedEventFromPlugin(WalletConnectUnitySession session)
    {
        string account = session.Accounts[0];
        MetamaskLoginSuccess(account);
    }

    private void SetDataToUser(GetPlayerDataResult res)
    {
        UserManager.Instance.SetPlayerUserName(res.nickname, false);
        UserManager.Instance.SetPlayerRank(res.rank);
        UserManager.Instance.SetWalletAddress(tempAddress);
    }

    public void GetOrCreatePlayer(string address)
    {
        tempAddress = address;
        ServerManager.Instance.GetPlayerDataFromServer(PlayerAPI.Get,
        (string data) =>
        {
            GetPlayerDataResult result = JsonUtility.FromJson<GetPlayerDataResult>(data);
            SetDataToUser(result);
            GoToMainMenu();
            
        }
        , address, 
        (string data) => {
            CreatePlayerData formData = new()
            {
                address = tempAddress,
            };
            string jsonFormData = JsonUtility.ToJson(formData);
            ServerManager.Instance.SendPlayerDataToServer(PlayerAPI.Create, jsonFormData, (string data) =>
            {
                GetPlayerDataResult result = JsonUtility.FromJson<GetPlayerDataResult>(data);
                SetDataToUser(result);
                //should not go to main menu if fail on login?
                //SceneManager.LoadScene(EnvironmentManager.Instance.GetSceneName());
            });

        });
    }

    public override void Disable()
    {
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
        Screens.Instance.PopScreen(gameScreenLogin);
        WalletConnect.Instance.NewSessionConnected.RemoveListener(OnNewWalletSessionConnectedEventFromPlugin);
        base.Disable();
    }
}