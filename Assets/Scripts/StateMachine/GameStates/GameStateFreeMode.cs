using UnityEngine.SceneManagement;

public class GameStateFreeMode : GameState
{
    private GameScreenRobotSelection gameScreenRobotSelection;

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
            default:
                break;
        }
    }

    private void GoToMainMenu()
    {
        stateMachine.PushState(new GameStateMainMenu());
    }
    public override void Disable()
    {
        Screens.Instance.PopScreen(gameScreenRobotSelection);
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
    }
}
