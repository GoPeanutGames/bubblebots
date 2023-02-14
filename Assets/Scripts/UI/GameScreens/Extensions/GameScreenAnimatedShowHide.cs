public class GameScreenAnimatedShowHide : GameScreen
{
    public void Show()
    {
        AnimatorComponent.SetTrigger("Show");
    }

    public void Hide()
    {
        AnimatorComponent.SetTrigger("Hide");
    }
}