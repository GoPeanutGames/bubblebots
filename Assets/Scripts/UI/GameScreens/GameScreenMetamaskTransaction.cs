using UnityEngine;

public class GameScreenMetamaskTransaction : GameScreen
{
    public GameObject loadingParent;
    public GameObject successParent;
    public GameObject failParent;

    private void ResetState()
    {
        loadingParent.SetActive(false);
        successParent.SetActive(false);
        failParent.SetActive(false);
    }

    public void SetLoading()
    {
        ResetState();
        loadingParent.SetActive(true);
    }

    public void SetSuccess()
    {
        ResetState();
        successParent.SetActive(true);
    }

    public void SetFail()
    {
        ResetState();
        failParent.SetActive(true);
    }
}
