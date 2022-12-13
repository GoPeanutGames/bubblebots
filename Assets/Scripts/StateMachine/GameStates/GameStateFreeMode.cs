using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateFreeMode : GameState
{
    private GameScreenRobotSelection gameScreenRobotSelection;
    private GameScreenGame gameScreenGame;


    private FreeToPlayGameplayManager freeToPlayGameplayManager;

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
            default:
                break;
        }
    }

    private void GoToMainMenu()
    {
        stateMachine.PushState(new GameStateMainMenu());
    }

    private void StartPlay()
    {
        Screens.Instance.PopScreen(gameScreenRobotSelection);
        gameScreenGame = Screens.Instance.PushScreen<GameScreenGame>();
        freeToPlayGameplayManager = GameObject.Instantiate(GameSettingsManager.Instance.freeToPlayGameplayManager).GetComponent<FreeToPlayGameplayManager>();

        freeToPlayGameplayManager.gameplayData = GameSettingsManager.Instance.freeToPlayGameplayData;
        freeToPlayGameplayManager.enemyDamage = GameSettingsManager.Instance.freeToPlayEnemyDamage;

        freeToPlayGameplayManager.StartSession();
    }


    public override void Disable()
    {
        Screens.Instance.PopScreen(gameScreenRobotSelection);
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
    }
}
