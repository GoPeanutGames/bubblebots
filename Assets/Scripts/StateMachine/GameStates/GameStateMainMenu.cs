public class GameStateMainMenu : GameState
{
    private GameScreenMainMenuBottomHUD gameScreenMainMenuBottomHUD;
    private GameScreenMainMenuTopHUD gameScreenMainMenuTopHUD;
    private GameScreenMainMenu gameScreenMainMenu;

    public override string GetGameStateName()
    {
        return "game state main menu";
    }

    public override void Enable()
    {
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
        gameScreenMainMenu = Screens.Instance.PushScreen<GameScreenMainMenu>();
        gameScreenMainMenuBottomHUD = Screens.Instance.PushScreen<GameScreenMainMenuBottomHUD>();
        gameScreenMainMenuTopHUD = Screens.Instance.PushScreen<GameScreenMainMenuTopHUD>();
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
            default:
                break;
        }
    }
    public override void Disable()
    {
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
    }
}
