using System.Runtime.InteropServices;
using BubbleBots.Server.Player;
using BubbleBots.Server.Signature;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

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
    private static extern void Login(bool isDev);

    [DllImport("__Internal")]
    private static extern void RequestSignature(string schema, string address);

    public override void Enable()
    {
        SoundManager.Instance.FadeInMusic();
        SoundManager.Instance.PlayStartMusicNew();
        gameScreenLogin = Screens.Instance.PushScreen<GameScreenLogin>();
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
        GameEventsManager.Instance.AddGlobalListener(OnMetamaskEvent);
        TryLoginFromSave();
    }

    private void TryLoginFromSave()
    {
        string address = UserManager.Instance.GetPlayerWalletAddress();
        string signature = UserManager.Instance.GetPlayerSignature();
        if (string.IsNullOrEmpty(address) == false && string.IsNullOrEmpty(signature) == false)
        {
            PostWeb3Login web3LoginData = new PostWeb3Login()
            {
                address = address,
                signature = signature
            };
            string data = JsonUtility.ToJson(web3LoginData);
            ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.Web3LoginCheck, data, (res) =>
            {
                ResponseWeb3Login loginResponse = JsonUtility.FromJson<ResponseWeb3Login>(res);
                if (loginResponse.status)
                {
                    StartLogin(address, signature);
                }
            });
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
                // PlayAsGuest();
                GoToMainMenu();
                UserManager.PlayerType = PlayerType.Guest;
                //UserManager.PlayerType = PlayerType.LoggedInUser;
                AnalyticsManager.Instance.InitAnalyticsGuest();
#else
                PlayAsGuest();
#endif
                break;
            case ButtonId.LoginGoogle:
                LoginWithGoogle();
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
        gameScreenLogin.HideLoadingScreen();
        GoToMainMenu();
        //stateMachine.PushState(new GameStateFreeMode());
    }

    private void GoToMainMenu()
    {
        stateMachine.PushState(new GameStateMainMenu());
    }

    private void LoginWithMetamask()
    {
        bool isDev = EnvironmentManager.Instance.IsDevelopment();
        Login(isDev);
    }

    private void LoginWithGoogle()
    {
        //LoginOnServerWithGoogleToken("");
        //return;

        gameScreenLogin.ShowLoadingScreen();
        var config = new PlayGamesClientConfiguration.Builder()
                    .AddOauthScope("profile")
                    .RequestEmail()
                    .RequestIdToken()
                    .RequestServerAuthCode(false)
                    .Build();

        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        Social.localUser.Authenticate(ProcessAuthentication);
    }

    internal void LoginOnServerWithGoogleToken(string token)
    {
        GoogleLogin loginData = new GoogleLogin()
        {
            accessToken = token
        };
        string formData = JsonUtility.ToJson(loginData);
        ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.GoogleLogin, formData,
            (success) =>
            {
                GoogleLoginResult result = JsonUtility.FromJson<GoogleLoginResult>(success);
                StartLogin(result.web3Info.address, result.web3Info.signature);
            },
            (fail) =>
            {
                gameScreenLogin.HideLoadingScreen();
            }
        );
    } 

    internal void ProcessAuthentication(bool success, string code)
    {
        if (success)
        {
            LoginOnServerWithGoogleToken(PlayGamesPlatform.Instance.GetIdToken());
        } 
        else
        {
            gameScreenLogin.HideLoadingScreen();
        }
    }

    public void MetamaskLoginSuccess(string address)
    {
        tempAddress = address;
        ServerManager.Instance.GetLoginSignatureDataFromServer(SignatureLoginAPI.Get, (schema) => { RequestSignatureFromMetamask(schema.ToString()); }, address);
        gameScreenLogin.ShowLoadingScreen();
        gameScreenLogin.HideLoginScreen();
    }

    private void RequestSignatureFromMetamask(string schema)
    {
        RequestSignature(schema, tempAddress);
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

    private void SetDataToUser(GetPlayerDataResult res)
    {
        UserManager.Instance.SetPlayerUserName(res.nickname, false);
        UserManager.Instance.SetPlayerRank(res.rank);
        UserManager.Instance.SetWalletAddress(tempAddress);
        UserManager.Instance.SetSignature(tempSignature);
    }

    private void GetOrCreatePlayer(string address, string signature)
    {
        //Debug.Log("ADDRESS " + address);
        //Debug.Log("SIGNATURE " + signature);
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
        base.Disable();
    }
}
