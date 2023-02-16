using System;

public class GameScreenAnimatedEntryExit : GameScreen
{
    private Action _closeCallback;

    public void StartOpen()
    {
        if (AnimatorComponent == null) return;
        AnimatorComponent.SetTrigger("Enter");
    }

    public void StartClose(Action closeSuccess = null)
    {
        _closeCallback = closeSuccess;
        if (AnimatorComponent == null) return;
        AnimatorComponent.SetTrigger("Exit");
    }

    public void OnCloseDone()
    {
        _closeCallback?.Invoke();
        Screens.Instance.PopScreen(this);
    }
}