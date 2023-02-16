using BubbleBots.Server.Player;
using UnityEngine;

public class GameStateFreeMode : GameState
{
    private GameScreenRobotSelection gameScreenRobotSelection;
    private GameScreenGame gameScreenGame;
    private GameScreenSkinsInfoPopup _gameScreenSkinsInfoPopup;
    
    private FreeToPlayGameplayManager freeToPlayGameplayManager;

    public override string GetGameStateName()
    {
        return "game state free mode";
    }

    public override void Enter()
    {
        gameScreenRobotSelection = Screens.Instance.PushScreen<GameScreenRobotSelection>();
        gameScreenRobotSelection.PopulateSelectionList(GameSettingsManager.Instance.freeModeGameplayData.robotsAvailable);
        SoundManager.Instance.FadeOutMusic(() =>
        {
            SoundManager.Instance.PlayRobotSelectMusicNew();
            SoundManager.Instance.FadeInMusic();
        });
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
    }

    public override void Enable()
    {
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
            stateMachine.PushState(new GameStateWonPopup(
                "<color=#FFCB5E>" + (data as GameEventLevelComplete).numBubblesWon.ToString() + "</color> Bubbles!",
                ButtonId.LevelCompleteContinue,
                "Continue",
                () => freeToPlayGameplayManager.StartNextLevel()) //todo: probably can do this better -> refactor
            );
        }
        else if (data.eventName == GameEvents.FreeModeLose)
        {
            stateMachine.PushState(new GameStateWonPopup(
                "<color=#FFCB5E>" + (data as GameEventFreeModeLose).numBubblesWon.ToString() + "</color> Bubbles from previous levels!",
                ButtonId.GameEndGoToMainMenu, 
                "Go to home"));
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
            case ButtonId.QuitGame:
                ShowQuitGameMenu();
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
        if (UserManager.PlayerType == PlayerType.Guest)
        {
            stateMachine.PopState();
        }
        else
        {
            stateMachine.PushState(new GameStateQuitPopup("You will not get your Energy back\nif you go back to the previous menu.\nAre you sure you want to go back?"));
        }
    }
    
    private void ShowQuitGameMenu()
    {
        if (freeToPlayGameplayManager.CanShowQuitPopup())
        {
            stateMachine.PushState(new GameStateQuitPopup("You will sacrifice your <color=#FFCB5E>Bubbles</color> this round... if you exit this game. Are you sure you want to quit?"));
        }
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
        
        if (UserManager.PlayerType == PlayerType.Guest)
        {
            Screens.Instance.PopScreen(gameScreenRobotSelection);
            stateMachine.PushState(new GameStateLogin());
            return;
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
        freeToPlayGameplayManager = GameObject.Instantiate(GameSettingsManager.Instance.freemodeGameplayManager).GetComponent<FreeToPlayGameplayManager>();

        freeToPlayGameplayManager.gameplayData = GameSettingsManager.Instance.freeModeGameplayData;
        freeToPlayGameplayManager.enemyDamage = GameSettingsManager.Instance.freeModeEnemyDamage;
        freeToPlayGameplayManager.serverGameplayController = ServerGameplayController.Instance;

        freeToPlayGameplayManager.StartSession(gameScreenRobotSelection.GetSelectedBots());
        UserManager.Instance.GetPlayerResources();
        Screens.Instance.SetGameBackground(GameSettingsManager.Instance.freeModeGameplayData.gamebackgroundSprite);
        Screens.Instance.PopScreen(gameScreenRobotSelection);
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
        UserManager.CallbackWithResources -= ResourcesReceived;
    }

    public override void Exit()
    {
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
        Screens.Instance.PopScreen(gameScreenRobotSelection);
        Screens.Instance.PopScreen(gameScreenGame);
        GoToMainMenu();
    }
}
