using TMPro;

public class GameQuitPopup : GameScreenAnimatedEntryExit
{
    public TextMeshProUGUI WarningText;

    public void SetWarningText(string text)
    {
        WarningText.text = text;
    }
}