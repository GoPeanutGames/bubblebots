using UnityEngine.SceneManagement;

public class GameStateFreeToPlay : GameState
{

    public override string GetGameStateName()
    {
        return "game state free to play";
    }

    public override void Enable()
    {
        SceneManager.LoadScene("FreeToPlayMode");
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
