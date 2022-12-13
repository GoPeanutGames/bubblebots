using System.Runtime.InteropServices;
using BubbleBots.Server.Player;
using BubbleBots.Server.Signature;
using UnityEngine;
using WalletConnectSharp.Unity;

public class GameStateLogin : GameState
{
    private GameScreenLogin gameScreenLogin;
    private string tempAddress;
    private string tempSignature;

    public override string GetGameStateName()
    {
        return "game state login";
    }

    [DllImport("__Internal")]
    private static extern void Login();

    [DllImport("__Internal")]
    private static extern void RequestSignature(string schema, string address);

    public override void Enable()
    {
        SoundManager.Instance.FadeInMusic();
        SoundManager.Instance.PlayStartMusicNew();
        gameScreenLogin = Screens.Instance.PushScreen<GameScreenLogin>();
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
        GameEventsManager.Instance.AddGlobalListener(OnMetamaskEvent);
        WalletConnect.Instance.NewSessionConnected.AddListener(OnNewWalletSessionConnectedEventFromPlugin);
        TryLoginFromSave();
    }

    private void TryLoginFromSave()
    {
        string address = UserManager.Instance.GetPlayerWalletAddress();
        string signature = UserManager.Instance.GetPlayerSignature();
        if (string.IsNullOrEmpty(address) == false && string.IsNullOrEmpty(signature) == false)
        {
            StartLogin(address, signature);
        }
    }

    private void OnGameEvent(GameEventData gameEvent)
    {
        if (gameEvent.eventName == GameEvents.ButtonTap)
        {
            OnButtonTap(gameEvent);
        }
    }

    private void OnMetamaskEvent(GameEventData data)
    {
        GameEventString metamaskEvent = data as GameEventString;
        if (data.eventName == GameEvents.MetamaskSuccess)
        {
            MetamaskLoginSuccess(metamaskEvent.stringData);
        }
        else if (data.eventName == GameEvents.SignatureSuccess)
        {
            SignatureLoginSuccess(metamaskEvent.stringData);
        }
    }

    private void OnButtonTap(GameEventData data)
    {
        GameEventString buttonTapData = data as GameEventString;
        switch (buttonTapData.stringData)
        {
            case ButtonId.LoginGuest:
                gameScreenLogin.OnPlayAsGuestPressed();
                break;
            case ButtonId.LoginGuestPlay:
#if UNITY_EDITOR
                PlayAsGuest();
                // GoToMainMenu();
                // UserManager.PlayerType = PlayerType.Guest;
                // AnalyticsManager.Instance.InitAnalyticsGuest();
#else
        
                PlayAsGuest();
#endif

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
        stateMachine.PushState(new GameStateFreeMode());
    }

    private void GoToMainMenu()
    {
        stateMachine.PushState(new GameStateMainMenu());
    }

    private void LoginWithMetamask()
    {
        if (Application.isMobilePlatform == false)
        {
            Login();
        }
        else
        {
            WalletConnect.Instance.OpenDeepLink();
        }
    }

    public void MetamaskLoginSuccess(string address)
    {
        tempAddress = address;
        ServerManager.Instance.GetLoginSignatureDataFromServer(SignatureLoginAPI.Get, (schema) => { RequestSignatureFromMetamask(schema.ToString()); }, address);
        gameScreenLogin.ShowLoadingScreen();
        gameScreenLogin.HideLoginScreen();
    }


    private async void RequestSignatureFromMetamask(string schema)
    {
        if (Application.isMobilePlatform)
        {
            string signature = await WalletConnect.ActiveSession.EthPersonalSign(tempAddress, schema);
            SignatureLoginSuccess(signature);
        }
        else
        {
            RequestSignature(schema, tempAddress);
        }
    }

    public void SignatureLoginSuccess(string signature)
    {
        SoundManager.Instance.PlayMetamaskSfx();
        StartLogin(tempAddress, signature);
    }

    private void StartLogin(string address, string signature)
    {
        SoundManager.Instance.PlayMetamaskSfx();
        AnalyticsManager.Instance.InitAnalyticsWithWallet(address);
        GetOrCreatePlayer(address, signature);
        UserManager.PlayerType = PlayerType.LoggedInUser;
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
        UserManager.Instance.SetSignature(tempSignature);
    }

    private void GetOrCreatePlayer(string address, string signature)
    {
        tempAddress = address;
        tempSignature = signature;
        ServerManager.Instance.GetPlayerDataFromServer(PlayerAPI.Get,
            (string data) =>
            {
                GetPlayerDataResult result = JsonUtility.FromJson<GetPlayerDataResult>(data);
                SetDataToUser(result);
                GoToMainMenu();
            }
            , address,
            (string data) =>
            {
                CreatePlayerData formData = new()
                {
                    address = tempAddress,
                    signature = tempSignature
                };
                string jsonFormData = JsonUtility.ToJson(formData);
                ServerManager.Instance.SendPlayerDataToServer(PlayerAPI.Create, jsonFormData, (string data) =>
                {
                    GetPlayerDataResult result = JsonUtility.FromJson<GetPlayerDataResult>(data);
                    SetDataToUser(result);
                    GoToMainMenu();
                });
            });
    }

    public override void Disable()
    {
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
        GameEventsManager.Instance.RemoveGlobalListener(OnMetamaskEvent);
        Screens.Instance.PopScreen(gameScreenLogin);
        WalletConnect.Instance.NewSessionConnected.RemoveListener(OnNewWalletSessionConnectedEventFromPlugin);
        base.Disable();
    }
}