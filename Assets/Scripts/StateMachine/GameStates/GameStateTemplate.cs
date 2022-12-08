public class GameStateTemplate : GameState
{

    public override string GetGameStateName()
    {
        return "game state template";
    }

    public override void Enable()
    {

        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
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
