using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateFreeMode : GameState
{
    private GameScreenRobotSelection gameScreenRobotSelection;
    private GameScreenGame gameScreenGame;
    private GameScreenLevelComplete gameScreenLevelComplete;
    private GameScreenGameEnd gameScreenGameEnd;

    private FreeToPlayGameplayManager freeToPlayGameplayManager;



    [DllImport("__Internal")]
    private static extern void Premint();

    public override string GetGameStateName()
    {
        return "game state free mode";
    }

    public override void Enable()
    {
        //SceneManager.LoadScene("FreeToPlayMode");
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
                freeToPlayGameplayManager.StartNextLevel();
                Screens.Instance.PopScreen(gameScreenLevelComplete);
                break;
            case ButtonId.GameEndPremint:
                PremintPressed();
                break;
            default:
                break;
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
        stateMachine.PushState(new GameStateMainMenu());
    }

    private void StartPlay()
    {
        
        gameScreenGame = Screens.Instance.PushScreen<GameScreenGame>();
        freeToPlayGameplayManager = GameObject.Instantiate(GameSettingsManager.Instance.freeToPlayGameplayManager).GetComponent<FreeToPlayGameplayManager>();

        freeToPlayGameplayManager.gameplayData = GameSettingsManager.Instance.freeToPlayGameplayData;
        freeToPlayGameplayManager.enemyDamage = GameSettingsManager.Instance.freeToPlayEnemyDamage;

        freeToPlayGameplayManager.StartSession(gameScreenRobotSelection.GetSelectedBots());

        Screens.Instance.PopScreen(gameScreenRobotSelection);
    }


    public override void Disable()
    {
        Screens.Instance.PopScreen(gameScreenRobotSelection);
        Screens.Instance.PopScreen(gameScreenGameEnd);
        Screens.Instance.PopScreen(gameScreenGame);
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
    }
}
