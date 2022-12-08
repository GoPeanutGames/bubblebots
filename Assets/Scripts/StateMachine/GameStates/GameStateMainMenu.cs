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


    private void OnGameEvent(string ev, object context)
    {
        if (ev == GameEvents.ButtonTap)
        {
            OnButtonTap(ev, context);
        }
    }

    private void OnButtonTap(string evt, object context)
    {

        CustomButtonData customButtonData = (CustomButtonData)context;
        switch (customButtonData.buttonId)
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
