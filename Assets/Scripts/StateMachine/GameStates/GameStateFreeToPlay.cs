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
