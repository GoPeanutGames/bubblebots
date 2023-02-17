public class GameScreenAnimatedShowHide : GameScreen
{
    private bool _shown = false;
    
    public void Show()
    {
        if (_shown) return;
        _shown = true;
        AnimatorComponent.SetTrigger("Show");
    }

    public void Hide()
    {
        if (!_shown) return;
        _shown = false;
        AnimatorComponent.SetTrigger("Hide");
    }
}