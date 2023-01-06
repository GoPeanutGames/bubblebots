using UnityEngine;

public class GameStateNetherMode : GameState
{
    private GameScreenRobotSelection gameScreenRobotSelection;
    private GameScreenGame gameScreenGame;
    private GameScreenLevelComplete gameScreenLevelComplete;
    private GameScreenGameEnd gameScreenGameEnd;
    private GameScreenQuitToMainMenu gameScreenQuitToMainMenu;
    private GameScreenMainMenuTopHUD _gameScreenMainMenuTopHUD;
    private GameScreenSkinsInfoPopup _gameScreenSkinsInfoPopup;
    private GameScreenRobotSelectQuit _gameScreenRobotSelectQuit;
    
    private NetherModeGameplayManager netherModeGameplayManager;

    public override string GetGameStateName()
    {
        return "game state nether mode";
    }

    public override void Enable()
    {
        Screens.Instance.SetBackground(GameSettingsManager.Instance.netherModeGameplayData.backgroundSprite);
        gameScreenRobotSelection = Screens.Instance.PushScreen<GameScreenRobotSelection>();
        _gameScreenMainMenuTopHUD = Screens.Instance.PushScreen<GameScreenMainMenuTopHUD>(true);
        _gameScreenMainMenuTopHUD.HidePlayerInfoGroup();
        gameScreenRobotSelection.PopulateSelectionList(GameSettingsManager.Instance.netherModeGameplayData.robotsAvailable);
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
        SoundManager.Instance?.FadeOutMusic(() =>
        {
            SoundManager.Instance.PlayRobotSelectMusicNew();
            SoundManager.Instance.FadeInMusic();
        });
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
            gameScreenLevelComplete.SetMessage("You earned " + (data as GameEventLevelComplete).lastLevelPotentialBubbles.ToString() + " bubbles!");
            gameScreenLevelComplete.SetButtonText("Continue");
        }
        else if (data.eventName == GameEvents.FreeModeLose)
        {
            gameScreenGameEnd = Screens.Instance.PushScreen<GameScreenGameEnd>();
            gameScreenGameEnd.SetMessage("You earned a total of <color=#FD78BE>" + (data as GameEventFreeModeLose).numBubblesWon.ToString() + "</color> Bubbles from previous levels!");
        }
        else if (data.eventName == GameEvents.UpdateSessionResponse)
        {
            netherModeGameplayManager.OnNewBubblesCount((data as GameEventUpdateSession).bubbles);
        } 
        else if (data.eventName == GameEvents.NetherModeComplete)
        {
            gameScreenLevelComplete = Screens.Instance.PushScreen<GameScreenLevelComplete>();
            gameScreenLevelComplete.SetMessage("You earned " + (data as GameEventNetherModeComplete).numBubblesWon.ToString() + " bubbles and extra 10,000 Bubbles for completing all levels!");
            gameScreenLevelComplete.SetButtonText("GO TO HOME");
        }
    }

    private void OnButtonTap(GameEventData data)
    {
        GameEventString customButtonData = data as GameEventString;
        switch (customButtonData.stringData)
        {
            case ButtonId.RobotSelectionBackButton:
                ShowQuitRobotSelect();
                break;
            case ButtonId.RobotSelectionQuestionMark:
                OpenSkinPopup();
                break;
            case ButtonId.RobotSelectionSkinPopupClose:
                CloseSkinPopup();
                break;
            case ButtonId.RobotSelectionStartButton:
                StartPlay();
                break;
            case ButtonId.LevelCompleteContinue:
                if (netherModeGameplayManager.IsFinished())
                {
                    GoToMainMenu();
                } 
                else
                { 
                    netherModeGameplayManager.StartNextLevel();
                    Screens.Instance.PopScreen(gameScreenLevelComplete);
                }
                break;
            case ButtonId.GameEndPremint:
                PremintPressed();
                break;
            case ButtonId.QuitGame:
                ShowQuitGameMenu();
                break;
            case ButtonId.QuitGameMenuPlay:
                ContinuePlaying();
                break;
            case ButtonId.QuitGameMenuQuit:
            case ButtonId.GameEndGoToMainMenu:
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
    
    private void ShowQuitRobotSelect()
    {
        _gameScreenRobotSelectQuit = Screens.Instance.PushScreen<GameScreenRobotSelectQuit>();
        _gameScreenRobotSelectQuit.SetWarningText("You will not get your Gem back\nif you go back to the previous menu.\nAre you sure you want to go back?");
    }
    
    private void ContinuePlaying()
    {
        Screens.Instance.PopScreen(gameScreenQuitToMainMenu);
        if (_gameScreenRobotSelectQuit != null)
        {
            Screens.Instance.PopScreen(_gameScreenRobotSelectQuit);
        }
    }

    private void ShowQuitGameMenu()
    {
        if (netherModeGameplayManager.CanShowQuitPopup())
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
        if (netherModeGameplayManager != null)
        {
            GameObject.Destroy(netherModeGameplayManager.gameObject);
            netherModeGameplayManager = null;
        }

        if (gameScreenGame != null)
        {
            gameScreenGame.GetComponent<GUIGame>().DestroyExplosionEffects();
        }

        if (_gameScreenRobotSelectQuit != null)
        {
            Screens.Instance.PopScreen(_gameScreenRobotSelectQuit);
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
        netherModeGameplayManager = GameObject.Instantiate(GameSettingsManager.Instance.netherModeGameplayManager).GetComponent<NetherModeGameplayManager>();

        netherModeGameplayManager.gameplayData = GameSettingsManager.Instance.netherModeGameplayData;
        netherModeGameplayManager.enemyDamage = GameSettingsManager.Instance.netherModeEnemyDamage;
        netherModeGameplayManager.serverGameplayController = ServerGameplayController.Instance;

        netherModeGameplayManager.StartSession(gameScreenRobotSelection.GetSelectedBots());

        Screens.Instance.PopScreen(gameScreenRobotSelection);
        Screens.Instance.PopScreen<GameScreenMainMenuTopHUD>();
        Screens.Instance.HideGameBackground();
    }


    public override void Disable()
    {
        Screens.Instance.PopScreen(gameScreenRobotSelection);
        Screens.Instance.PopScreen(gameScreenGameEnd);
        Screens.Instance.PopScreen(gameScreenGame);
        Screens.Instance.PopScreen(gameScreenQuitToMainMenu);
        Screens.Instance.PopScreen(gameScreenLevelComplete);
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
    }
}
