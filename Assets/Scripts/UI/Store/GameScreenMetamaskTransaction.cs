using TMPro;
using UnityEngine;

public class GameScreenMetamaskTransaction : GameScreen
{
    public GameObject loadingParent;
    public GameObject successParent;
    public GameObject failParent;
    public TextMeshProUGUI FailExplanationText;

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

    public void SetFail(string reason)
    {
        if (reason == "error")
        {
            FailExplanationText.text = "Unexpected error occured, try Again.";
        }
        else if (reason == "balance")
        {
            FailExplanationText.text = "Not enough balance.";
        }
        else if (reason == "cancelled")
        {
            FailExplanationText.text = "User cancelled.";
        }

        ResetState();
        failParent.SetActive(true);
    }
}