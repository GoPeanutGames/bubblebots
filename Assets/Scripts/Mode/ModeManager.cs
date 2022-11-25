using BubbleBots.Modes;

public class ModeManager : MonoSingleton<ModeManager>
{
    public Mode Mode { get; private set; }

    public void SetMode(Mode newMode)
    {
        Mode = newMode;
    }
}
