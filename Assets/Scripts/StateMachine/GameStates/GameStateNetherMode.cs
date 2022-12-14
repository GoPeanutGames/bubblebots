using System.Runtime.InteropServices;
using UnityEngine;

public class GameStateNetherMode : GameState
{
    private GameScreenRobotSelection gameScreenRobotSelection;
    private GameScreenGame gameScreenGame;
    private GameScreenLevelComplete gameScreenLevelComplete;
    private GameScreenGameEnd gameScreenGameEnd;
    private GameScreenQuitToMainMenu gameScreenQuitToMainMenu;

    private NetherModeGameplayManager netherModeGameplayManager;

    [DllImport("__Internal")]
    private static extern void Premint();

    public override string GetGameStateName()
    {
        return "game state free mode";
    }

    public override void Enable()
    {
        gameScreenRobotSelection = Screens.Instance.PushScreen<GameScreenRobotSelection>();
        gameScreenRobotSelection.PopulateSelectionList();
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
    }


    private void OnGameEvent(GameEventData data)
    {
        if (data.eventName == GameEvents.ButtonTap)
        {
            OnButtonTap(data);
        } 
        else if (data.eventName == GameEvents.FreeModeLevelComplete)
        {
            gameScreenLevelComplete = Screens.Instance.PushScreen<GameScreenLevelComplete>();
            gameScreenLevelComplete.SetMessage("You won " + (data as GameEventLevelComplete).numBubblesWon.ToString() + " bubbles!");
        }
        else if (data.eventName == GameEvents.FreeModeLose)
        {
            gameScreenGameEnd = Screens.Instance.PushScreen<GameScreenGameEnd>();
            gameScreenGameEnd.SetScore((data as GameEventFreeModeLose).score.ToString());
        }
    }

    private void OnButtonTap(GameEventData data)
    {
        GameEventString customButtonData = data as GameEventString;
        switch (customButtonData.stringData)
        {
            case ButtonId.RobotSelectionBackButton:
                GoToMainMenu();
                break;
            case ButtonId.RobotSelectionStartButton:
                StartPlay();
                break;
            case ButtonId.LevelCompleteContinue:
                netherModeGameplayManager.StartNextLevel();
                Screens.Instance.PopScreen(gameScreenLevelComplete);
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
                GoToMainMenu();
                break;
            default:
                break;
        }
    }

    private void ContinuePlaying()
    {
        Screens.Instance.PopScreen(gameScreenQuitToMainMenu);
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
                Premint();
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

        stateMachine.PushState(new GameStateMainMenu());
    }

    private void StartPlay()
    {
        Screens.Instance.SetBackground(GameSettingsManager.Instance.netherModeGameplayData.backgroundSprite);
        gameScreenGame = Screens.Instance.PushScreen<GameScreenGame>();
        netherModeGameplayManager = GameObject.Instantiate(GameSettingsManager.Instance.netherModeGameplayManager).GetComponent<NetherModeGameplayManager>();

        netherModeGameplayManager.gameplayData = GameSettingsManager.Instance.netherModeGameplayData;
        netherModeGameplayManager.enemyDamage = GameSettingsManager.Instance.netherModeEnemyDamage;
        //netherModeGameplayManager.serverGameplayController = ServerGameplayController.Instance;

        netherModeGameplayManager.StartSession(gameScreenRobotSelection.GetSelectedBots());

        Screens.Instance.PopScreen(gameScreenRobotSelection);
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
