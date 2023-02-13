using System;

public class GameScreenPopup : GameScreen
{
    private Action _closeCallback;

    public void StartOpen()
    {
        AnimatorComponent.SetTrigger("Enter");
    }

    public void StartClose(Action closeSuccess = null)
    {
        _closeCallback = closeSuccess;
        AnimatorComponent.SetTrigger("Exit");
    }

    public void OnCloseDone()
    {
        _closeCallback?.Invoke();
        Screens.Instance.PopScreen(this);
    }
}