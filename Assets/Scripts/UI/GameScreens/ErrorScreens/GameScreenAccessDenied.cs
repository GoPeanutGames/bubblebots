using System;

public class GameScreenAccessDenied : GameScreen
{
    private Action _closeCallback;

    public void StartOpen()
    {
        AnimatorComponent.SetTrigger("Enter");
    }

    public void StartClose(Action closeSuccess)
    {
        _closeCallback = closeSuccess;
        AnimatorComponent.SetTrigger("Exit");
    }

    public void OnCloseDone()
    {
        _closeCallback?.Invoke();
    }
}