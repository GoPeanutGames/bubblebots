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

    public override void Enable()
    {
        SoundManager.Instance.FadeInMusic();
        SoundManager.Instance.PlayStartMusicNew();
        gameScreenLogin = Screens.Instance.PushScreen<GameScreenLogin>();
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
        //todo: figure stuff out for login
        if (false)
        {
            TryLoginFromSave();
        }
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

    private void OnButtonTap(GameEventData data)
    {
        GameEventString buttonTapData = data as GameEventString;
        switch (buttonTapData.stringData)
        {
            case ButtonId.LoginGuest:
                gameScreenLogin.OnPlayAsGuestPressed();
                break;
            case ButtonId.LoginMobileDownload:
                Application.OpenURL("https://peanutgames.com/");
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

    private void LoginWithGoogle()
    {
#if UNITY_ANDROID
        gameScreenLogin.ShowLoadingScreen();
        var config = new PlayGamesClientConfiguration.Builder()
            .AddOauthScope("profile")
            .AddOauthScope("email")
            .RequestEmail()
            .RequestIdToken()
            .RequestServerAuthCode(false)
            .Build();

        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();


        Social.localUser.Authenticate(ProcessAuthentication);
#endif
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
                Debug.Log("login server failed");
                gameScreenLogin.HideLoadingScreen();
            }
        );
    }

    internal void ProcessAuthentication(bool success, string code)
    {
#if UNITY_ANDROID
        if (success)
        {
            Debug.Log("google token " + PlayGamesPlatform.Instance.GetIdToken());
            LoginOnServerWithGoogleToken(PlayGamesPlatform.Instance.GetIdToken());
        }
        else
        {
            Debug.Log("google failed");
            gameScreenLogin.HideLoadingScreen();
        }
#endif
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
        Debug.Log("ADDRESS " + address);
        Debug.Log("SIGNATURE " + signature);
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
        Screens.Instance.PopScreen(gameScreenLogin);
        base.Disable();
    }
}