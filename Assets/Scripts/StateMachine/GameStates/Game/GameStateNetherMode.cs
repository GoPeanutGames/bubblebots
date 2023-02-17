using UnityEngine;

public class GameStateNetherMode : GameState
{
    private GameScreenRobotSelection gameScreenRobotSelection;
    private GameScreenGame gameScreenGame;
    private GameScreenSkinsInfoPopup _gameScreenSkinsInfoPopup;
    
    private NetherModeGameplayManager netherModeGameplayManager;

    public override string GetGameStateName()
    {
        return "game state nether mode";
    }

    public override void Enter()
    {
        gameScreenRobotSelection = Screens.Instance.PushScreen<GameScreenRobotSelection>();
        gameScreenRobotSelection.PopulateSelectionList(GameSettingsManager.Instance.netherModeGameplayData.robotsAvailable);
        SoundManager.Instance.FadeOutMusic(() =>
        {
            SoundManager.Instance.PlayRobotSelectMusicNew();
            SoundManager.Instance.FadeInMusic();
        });
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
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
            stateMachine.PushState(new GameStateWonPopup(
                "<color=#FFCB5E>" + (data as GameEventLevelComplete).lastLevelPotentialBubbles.ToString() + "</color> Bubbles!",
                ButtonId.LevelCompleteContinue,
                "Continue",
                ()=>netherModeGameplayManager.StartNextLevel()
                ));
        }
        else if (data.eventName == GameEvents.FreeModeLose)
        {
            stateMachine.PushState(new GameStateWonPopup(
                "<color=#FFCB5E>" + (data as GameEventFreeModeLose).numBubblesWon.ToString() + "</color> Bubbles from previous levels!",
                ButtonId.GameEndGoToMainMenu,
                "Go to home"
            ));
        }
        else if (data.eventName == GameEvents.UpdateSessionResponse)
        {
            netherModeGameplayManager.OnNewBubblesCount((data as GameEventUpdateSession).bubbles);
        } 
        else if (data.eventName == GameEvents.NetherModeComplete)
        {
            stateMachine.PushState(new GameStateWonPopup(
                "You earned <color=#FFCB5E>" + (data as GameEventNetherModeComplete).numBubblesWon.ToString() + "</color> Bubbles and extra <color=#FFCB5E>10,000</color> Bubbles for completing all levels!",
                ButtonId.GameEndGoToMainMenu,
                "Go to home"
            ));
        }
    }

    private void OnButtonTap(GameEventData data)
    {
        GameEventString customButtonData = data as GameEventString;
        switch (customButtonData.stringData)
        {
            case ButtonId.RobotSelectionBackButton:
                ShowQuit();
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
            case ButtonId.QuitGame:
                ShowQuitGameMenu();
                break;
        }
    }

    private void OpenSkinPopup()
    {
        _gameScreenSkinsInfoPopup = Screens.Instance.PushScreen<GameScreenSkinsInfoPopup>();
        _gameScreenSkinsInfoPopup.StartOpen();
    }

    private void CloseSkinPopup()
    {
        _gameScreenSkinsInfoPopup.StartClose();
    }
    
    private void ShowQuitGameMenu()
    {
        if (netherModeGameplayManager.CanShowQuitPopup())
        {
            ShowQuit();
        }
    }
    
    private void ShowQuit()
    {
        stateMachine.PushState(new GameStateQuitPopup("You will not get your Gem back\nif you go back to the previous menu.\nAre you sure you want to go back?"));
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

        stateMachine.PushState(new GameStateHome());
        SoundManager.Instance.FadeOutMusic(() =>
        {
            SoundManager.Instance.PlayStartMusicNew();
            SoundManager.Instance.FadeInMusic();
        });
    }

    private void StartPlay()
    {
        
        gameScreenGame = Screens.Instance.PushScreen<GameScreenGame>();
        gameScreenGame.SetPlayerName(UserManager.Instance.GetPlayerUserName());
        netherModeGameplayManager = GameObject.Instantiate(GameSettingsManager.Instance.netherModeGameplayManager).GetComponent<NetherModeGameplayManager>();

        netherModeGameplayManager.gameplayData = GameSettingsManager.Instance.netherModeGameplayData;
        netherModeGameplayManager.enemyDamage = GameSettingsManager.Instance.netherModeEnemyDamage;
        netherModeGameplayManager.serverGameplayController = ServerGameplayController.Instance;

        netherModeGameplayManager.StartSession(gameScreenRobotSelection.GetSelectedBots());

        Screens.Instance.PopScreen(gameScreenRobotSelection);
        Screens.Instance.PopScreen<GameScreenHomeHeader>();
        Screens.Instance.HideGameBackground();
    }

    public override void Exit()
    {
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
        Screens.Instance.PopScreen(gameScreenRobotSelection);
        Screens.Instance.PopScreen(gameScreenGame);
        GoToMainMenu();
    }
}
