using BubbleBots.Server.Player;
using UnityEngine;

public class GameStateHome : GameState
{
    private GameScreenHomeHeader _gameScreenHomeHeader;
    private GameScreenHomeSideBar _gameScreenHomeSideBar;
    private GameScreenHome _gameScreenHome;
    private GameScreenLoading _gameScreenLoading;
    private GameScreenNotEnoughGems _gameScreenNotEnoughGems;
    private GameScreenNotLoggedIn _gameScreenNotLoggedIn;
    private GameScreenNotReferred _gameScreenNotReferred;
    private GameScreenLevelsMap _gameScreenLevelsMap;

    private bool navigateToLevelsMap = false;

    public GameStateHome(bool showLevelsMap = false)
    {
       navigateToLevelsMap = showLevelsMap;
    }
    public override string GetGameStateName()
    {
        return "game state home";
    }

    public override void Enter()
    {
        base.Enter();
        SoundManager.Instance.FadeInMusic();
        SoundManager.Instance.PlayHomeMusic();
        _gameScreenHome = Screens.Instance.PushScreen<GameScreenHome>(true);
        _gameScreenHomeHeader = Screens.Instance.PushScreen<GameScreenHomeHeader>(true);
        _gameScreenHomeSideBar = Screens.Instance.PushScreen<GameScreenHomeSideBar>(true);
    }

    public override void Enable()
    {
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
        UserManager.Instance.GetPlayerResources();

        if (navigateToLevelsMap)
        {
            ShowLevelsMap();
            navigateToLevelsMap = false;
        }
        else
        {
            _gameScreenHomeSideBar.Show();
            _gameScreenHomeHeader.Show();
            _gameScreenHomeHeader.RefreshData();
            if (UserManager.PlayerType == PlayerType.Guest)
            {
                if (UserManager.ShownOnce == false && UserManager.TimesPlayed == 2)
                {
                    UserManager.TimesPlayed = 0;
                    UserManager.ShownOnce = true;
                    stateMachine.PushState(new GameStateSaveYourProgress());
                }
                else if (UserManager.TimesPlayed == 5)
                {
                    UserManager.TimesPlayed = 0;
                    stateMachine.PushState(new GameStateSaveYourProgress());
                }
            }
        }
    }

    private void OnGameEvent(GameEventData data)
    {
        if (data.eventName == GameEvents.ButtonTap)
        {
            OnButtonTap(data);
        }
    }

    private void ClearStatesAndScreens()
    {
        while (Screens.GetCurrentScreen() != null)
        {
            Screens.Instance.PopScreen();
        }
        stateMachine.PopAll();
    }

    private void OnButtonTap(GameEventData data)
    {
        GameEventString customButtonData = data as GameEventString;
        switch (customButtonData.stringData)
        {
            case ButtonId.MainMenuBottomHUDPlay:
                ClearStatesAndScreens();
                stateMachine.PushState(new GameStateFreeMode());
                SoundManager.Instance.PlayModeSelectedSfx();
                break;
            case ButtonId.MainMenuTopHUDGemPlus:
            case ButtonId.MainMenuBottomHUDStore:
                ShowStore();
                break;
            case ButtonId.MainMenuBottomHUDBattlepass:
                stateMachine.PushState(new GameStateBattlepassPopup());
                break;
            case ButtonId.MainMenuSideBarLeaderboard:
                ShowLeaderboard();
                break;
            case ButtonId.ModeSelectNethermode:
                NethermodeClick();
                break;
            case ButtonId.MainMenuSideBarDashboard:
                Application.OpenURL("https://peanutgames.com/dashboard");
                break;
            case ButtonId.MainMenuSideBarUsePoints:
                Application.OpenURL("https://peanutgames.com/dashboard");
                break;
            case ButtonId.HomeHeaderExplanator:
                stateMachine.PushState(new GameStateExplanatorPopup());
                break;
            case ButtonId.MainMenuSideBarSettings:
                stateMachine.PushState(new GameStateOptions());
                break;
            case ButtonId.NotEnoughGemsBack:
                Screens.Instance.PopScreen(_gameScreenNotEnoughGems);
                break;
            case ButtonId.NotEnoughGemsBuy:
                Screens.Instance.PopScreen(_gameScreenNotEnoughGems);
                ShowStore();
                break;
            case ButtonId.NotLoggedIn:
                UserManager.Instance.loginManager.MetamaskSignIn(OnMetamaskLoginSuccess, null);
                break;
            case ButtonId.NotLoggedInClose:
                Screens.Instance.PopScreen(_gameScreenNotLoggedIn);
                break;
            case ButtonId.NotReferredClose:
            case ButtonId.NotReferredGetReferral:
                Screens.Instance.PopScreen(_gameScreenNotReferred);
                break;
            case ButtonId.MainMenuBottomHUDLevels:
                ShowLevelsMap();
                break;
            case ButtonId.LevelsMapBack:
                HideLevelsMap();
                break;
            case ButtonId.LevelsMapPlay:
                PlayCurrentLevel();
                break;
            default:
                break;
        }
    }


    private void PlayCurrentLevel()
    {
        ClearStatesAndScreens();
        stateMachine.PushState(new GameStateLevelsMode());
        SoundManager.Instance.PlayModeSelectedSfx();
    }


    private void ShowLevelsMap()
    {
        _gameScreenHomeSideBar.Hide();
        _gameScreenHomeHeader.Hide();
        _gameScreenLevelsMap = Screens.Instance.PushScreen<GameScreenLevelsMap>(true);
        _gameScreenLevelsMap.SetPlayButtonText("LEVEL " + UserManager.Instance.GetCurrentLevel().ToString());
    }

    private void HideLevelsMap()
    {
        _gameScreenHomeSideBar.Show();
        _gameScreenHomeHeader.Show();
        Screens.Instance.PopScreen<GameScreenLevelsMap>();
    }


    private void OnMetamaskLoginSuccess()
    {
        _gameScreenHomeHeader.RefreshData();
        Screens.Instance.PopScreen(_gameScreenNotLoggedIn);
        UserManager.Instance.GetPlayerResources();
        if (UserManager.PlayerType == PlayerType.Guest)
        {
            if (UserManager.ShownOnce == false && UserManager.TimesPlayed == 2)
            {
                UserManager.TimesPlayed = 0;
                UserManager.ShownOnce = true;
                stateMachine.PushState(new GameStateSaveYourProgress());
            }
            else if (UserManager.TimesPlayed == 5)
            {
                UserManager.TimesPlayed = 0;
                stateMachine.PushState(new GameStateSaveYourProgress());
            }
        }
    }

    private void NethermodeClick()
    {
        if (!LoggedInReferredLock())
        {
            _gameScreenLoading = Screens.Instance.PushScreen<GameScreenLoading>();
            CheckForBattlePass();
        }
    }

    private void CheckForBattlePass()
    {
        ServerManager.Instance.GetPlayerDataFromServer(PlayerAPI.Battlepass, BattlePassSuccess, UserManager.Instance.GetPlayerWalletAddress(), BattlePassFail);
    }

    private void BattlePassSuccess(string data)
    {
        GetBattlePassResponse response = JsonUtility.FromJson<GetBattlePassResponse>(data);
        if (response.exists)
        {
            PlayNetherMode();
        }
        else
        {
            BattlePassFail("no battlepass");
        }
    }

    private void BattlePassFail(string data)
    {
        UserManager.CallbackWithResources += ResourcesReceived;
        UserManager.Instance.GetPlayerResources();
    }

    private void ResourcesReceived(GetPlayerWallet wallet)
    {
        Screens.Instance.PopScreen(_gameScreenLoading);
        UserManager.CallbackWithResources -= ResourcesReceived;
        if (wallet.gems <= 0)
        {
            _gameScreenNotEnoughGems = Screens.Instance.PushScreen<GameScreenNotEnoughGems>();
            return;
        }
        PlayNetherMode();
    }

    private void PlayNetherMode()
    {
        ClearStatesAndScreens();
        stateMachine.PushState(new GameStateNetherMode());
    }

    private void ShowLeaderboard()
    {
        _gameScreenHomeHeader.Hide();
        _gameScreenHomeSideBar.Hide();
        stateMachine.PushState(new GameStateLeaderboard());
    }

    private bool LoggedInReferredLock()
    {
        if (!UserManager.Instance.IsLoggedIn)
        {
            _gameScreenNotLoggedIn = Screens.Instance.PushScreen<GameScreenNotLoggedIn>();
            return true;
        }
        else if (!UserManager.Instance.IsReferred)
        {
            _gameScreenNotReferred = Screens.Instance.PushScreen<GameScreenNotReferred>();
            return true;
        }
        return false;
    }

    private void ShowStore()
    {
        if (!LoggedInReferredLock())
        {
            stateMachine.PushState(new GameStateStore());
        }
    }

    public override void Disable()
    {
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
    }

    public override void Exit()
    {
        Screens.Instance.PopScreen(_gameScreenHome);
        Screens.Instance.PopScreen(_gameScreenHomeHeader);
        Screens.Instance.PopScreen(_gameScreenHomeSideBar);
    }
}