using BubbleBots.Server.Player;
using UnityEngine;

public class GameStateFreeMode : GameState
{
    private GameScreenRobotSelection gameScreenRobotSelection;
    private GameScreenGame gameScreenGame;
    private GameScreenLevelComplete gameScreenLevelComplete;
    private GameScreenGameEnd gameScreenGameEnd;
    private GameScreenQuitToMainMenu gameScreenQuitToMainMenu;
    private GameScreenMainMenuTopHUD _gameScreenMainMenuTopHUD;
    private GameScreenSkinsInfoPopup _gameScreenSkinsInfoPopup;
    private GameScreenRobotSelectQuit _gameScreenRobotSelectQuit;
    
    private FreeToPlayGameplayManager freeToPlayGameplayManager;

    public override string GetGameStateName()
    {
        return "game state free mode";
    }

    public override void Enable()
    {
        gameScreenRobotSelection = Screens.Instance.PushScreen<GameScreenRobotSelection>();
        if (UserManager.PlayerType != PlayerType.Guest)
        {
            _gameScreenMainMenuTopHUD = Screens.Instance.PushScreen<GameScreenMainMenuTopHUD>(true);
            _gameScreenMainMenuTopHUD.DisablePlusButton();
            _gameScreenMainMenuTopHUD.HidePlayerInfoGroup();
        }
        Screens.Instance.SetBackground(GameSettingsManager.Instance.freeModeGameplayData.backgroundSprite);
        gameScreenRobotSelection.PopulateSelectionList(GameSettingsManager.Instance.freeModeGameplayData.robotsAvailable);
        Screens.Instance.HideGameBackground();
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
        SoundManager.Instance?.FadeOutMusic(() =>
        {
            SoundManager.Instance.PlayRobotSelectMusicNew();
            SoundManager.Instance.FadeInMusic();
        });
        UserManager.CallbackWithResources += ResourcesReceived;
    }

    private void OnGameEvent(GameEventData data)
    {
        if (data.eventName == GameEvents.ButtonTap)
        {
            OnButtonTap(data);
        }
        else if (data.eventName == GameEvents.FreeModeSessionStarted)
        {
            gameScreenGame.InitialiseEnemyRobots();
        }
        else if (data.eventName == GameEvents.FreeModeLevelStart)
        {
            GameEventLevelStart eventLevelStart = data as GameEventLevelStart;
            gameScreenGame.SetPlayerRobots(eventLevelStart.playerRoster);
            gameScreenGame.SetEnemyRobots(eventLevelStart.enemies);
        }
        else if (data.eventName == GameEvents.FreeModeEnemyRobotDamage)
        {
            GameEventEnemyRobotDamage eventEnemyRobotDamage = data as GameEventEnemyRobotDamage;
            gameScreenGame.DamageEnemyRobotAndSetHp(eventEnemyRobotDamage.index, eventEnemyRobotDamage.enemyRobotNewHp);
        }
        else if (data.eventName == GameEvents.FreeModePlayerRobotDamage)
        {
            GameEventPlayerRobotDamage eventPlayerRobotDamage = data as GameEventPlayerRobotDamage;
            gameScreenGame.DamagePlayerRobotAndSetHp(eventPlayerRobotDamage.id, eventPlayerRobotDamage.enemyIndex,eventPlayerRobotDamage.damage);
        }
        else if (data.eventName == GameEvents.FreeModePlayerRobotKilled)
        {
            GameEventPlayerRobotKilled eventPlayerRobotKilled = data as GameEventPlayerRobotKilled;
            gameScreenGame.KillPlayerRobot(eventPlayerRobotKilled.id, eventPlayerRobotKilled.enemyIndex);
        }
        else if (data.eventName == GameEvents.FreeModeEnemyRobotKilled)
        {
            GameEventEnemyRobotKilled eventEnemyRobotKilled = data as GameEventEnemyRobotKilled;
            gameScreenGame.KillEnemyRobot(eventEnemyRobotKilled.id);
        }
        else if (data.eventName == GameEvents.FreeModeEnemyRobotTargeted)
        {
            GameEventEnemyRobotTargeted eventEnemyRobotTargeted = data as GameEventEnemyRobotTargeted;
            gameScreenGame.TargetEnemyRobot(eventEnemyRobotTargeted.id);
        }
        else if (data.eventName == GameEvents.FreeModeEnemyChanged)
        {
            GameEventInt eventEnemyRobotTargeted = data as GameEventInt;
            gameScreenGame.TargetEnemyRobot(eventEnemyRobotTargeted.intData);
        }
        else if (data.eventName == GameEvents.FreeModeLevelComplete)
        {
            gameScreenLevelComplete = Screens.Instance.PushScreen<GameScreenLevelComplete>();
            gameScreenLevelComplete.SetMessage("You earned " + (data as GameEventLevelComplete).numBubblesWon.ToString() + " bubbles!");
            gameScreenLevelComplete.SetButtonText("Continue");
        }
        else if (data.eventName == GameEvents.FreeModeLose)
        {
            gameScreenGameEnd = Screens.Instance.PushScreen<GameScreenGameEnd>();
            gameScreenGameEnd.SetMessage("You earned a total of <color=#FD78BE>" + (data as GameEventFreeModeLose).numBubblesWon.ToString() + "</color> Bubbles from previous levels!");
        }
        else if (data.eventName == GameEvents.UpdateSessionResponse)
        {
            if (freeToPlayGameplayManager != null)
            {
                freeToPlayGameplayManager.OnNewBubblesCount((data as GameEventUpdateSession).bubbles);
            }
        }
    }

    private void OnButtonTap(GameEventData data)
    {
        GameEventString customButtonData = data as GameEventString;
        switch (customButtonData.stringData)
        {
            case ButtonId.RobotSelectionStartButton:
                StartPlay();
                break;
            case ButtonId.LevelCompleteContinue:
                freeToPlayGameplayManager.StartNextLevel();
                Screens.Instance.PopScreen(gameScreenLevelComplete);
                break;
            case ButtonId.QuitGame:
                ShowQuitGameMenu();
                break;
            case ButtonId.QuitGameMenuPlay:
                ContinuePlaying();
                break;
            case ButtonId.RobotSelectionQuestionMark:
                OpenSkinPopup();
                break;
            case ButtonId.RobotSelectionSkinPopupClose:
                CloseSkinPopup();
                break;
            case ButtonId.RobotSelectionBackButton:
                ShowQuitRobotSelect();
                break;
            case ButtonId.GameEndGoToMainMenu:
            case ButtonId.QuitGameMenuQuit:
                GoToMainMenu();
                break;
            default:
                break;
        }
    }

    private void OpenSkinPopup()
    {
        _gameScreenSkinsInfoPopup = Screens.Instance.PushScreen<GameScreenSkinsInfoPopup>();
    }

    private void CloseSkinPopup()
    {
        Screens.Instance.PopScreen(_gameScreenSkinsInfoPopup);
    }

    private void ContinuePlaying()
    {
        Screens.Instance.PopScreen(gameScreenQuitToMainMenu);
        if (_gameScreenRobotSelectQuit != null)
        {
            Screens.Instance.PopScreen(_gameScreenRobotSelectQuit);
        }
    }

    private void ShowQuitRobotSelect()
    {
        if (UserManager.PlayerType == PlayerType.Guest)
        {
            GoToMainMenu();
        }
        else
        {
            _gameScreenRobotSelectQuit = Screens.Instance.PushScreen<GameScreenRobotSelectQuit>();
            _gameScreenRobotSelectQuit.SetWarningText("You will not get your Energy back\nif you go back to the previous menu.\nAre you sure you want to go back?");
        }
    }
    
    private void ShowQuitGameMenu()
    {
        if (freeToPlayGameplayManager.CanShowQuitPopup())
        {
            gameScreenQuitToMainMenu = Screens.Instance.PushScreen<GameScreenQuitToMainMenu>();
        }
    }

    private void PremintPressed()
    {
#if !UNITY_EDITOR
                Application.OpenURL("https://www.premint.xyz/peanutgames-bubble-bots-mini-game/");
#endif
        GoToMainMenu();
        //        Menu.gameObject.SetActive(true);
        //        Menu.GetComponent<CanvasGroup>().DOFade(1, 0.35f);
        //        if (UserManager.PlayerType == PlayerType.Guest)
        //        {
        //            //Menu.transform.Find("PlayerLogin").gameObject.SetActive(true);
        //            SceneManager.LoadScene("Login");
        //        }
        //        else
        //        {
        //            Menu.transform.Find("PlayerInfo").gameObject.SetActive(true);
        //            Menu.DisplayHighScores();
        //            Menu.ReverseHighScoreButtons();
        //        }

        //        gameObject.SetActive(false);
    }

    private void GoToMainMenu()
    {
        if (freeToPlayGameplayManager != null)
        {
            GameObject.Destroy(freeToPlayGameplayManager.gameObject);
            freeToPlayGameplayManager = null;
        }

        if (gameScreenGame != null)
        {
            gameScreenGame.GetComponent<GUIGame>().DestroyExplosionEffects();
        }
        
        if (_gameScreenRobotSelectQuit != null)
        {
            Screens.Instance.PopScreen(_gameScreenRobotSelectQuit);
        }
        
        if (UserManager.PlayerType == PlayerType.Guest)
        {
            Screens.Instance.PopScreen(gameScreenRobotSelection);
            Screens.Instance.ResetBackground();
            stateMachine.PushState(new GameStateLogin());
            return;
        }

        stateMachine.PushState(new GameStateMainMenu());
        SoundManager.Instance.FadeOutMusic(() =>
        {
            SoundManager.Instance.PlayStartMusicNew();
            SoundManager.Instance.FadeInMusic();
        });
    }

    private void StartPlay()
    {
        gameScreenGame = Screens.Instance.PushScreen<GameScreenGame>();
        freeToPlayGameplayManager = GameObject.Instantiate(GameSettingsManager.Instance.freemodeGameplayManager).GetComponent<FreeToPlayGameplayManager>();

        freeToPlayGameplayManager.gameplayData = GameSettingsManager.Instance.freeModeGameplayData;
        freeToPlayGameplayManager.enemyDamage = GameSettingsManager.Instance.freeModeEnemyDamage;
        freeToPlayGameplayManager.serverGameplayController = ServerGameplayController.Instance;

        freeToPlayGameplayManager.StartSession(gameScreenRobotSelection.GetSelectedBots());
        UserManager.Instance.GetPlayerResources();
        Screens.Instance.SetGameBackground(GameSettingsManager.Instance.freeModeGameplayData.gamebackgroundSprite);
        Screens.Instance.PopScreen(gameScreenRobotSelection);
        //Screens.Instance.PopScreen<GameScreenMainMenuTopHUD>();
    }

    private void ResourcesReceived(GetPlayerWallet wallet)
    {
        if (freeToPlayGameplayManager != null)
        {
            freeToPlayGameplayManager.SetCanSpawnBubbles(wallet.energy > 0);
        }
    }


    public override void Disable()
    {
        Screens.Instance.PopScreen(gameScreenRobotSelection);
        Screens.Instance.PopScreen(gameScreenGameEnd);
        Screens.Instance.PopScreen(gameScreenGame);
        Screens.Instance.PopScreen(gameScreenQuitToMainMenu);
        Screens.Instance.PopScreen(gameScreenLevelComplete);
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
        UserManager.CallbackWithResources -= ResourcesReceived;
    }
}
