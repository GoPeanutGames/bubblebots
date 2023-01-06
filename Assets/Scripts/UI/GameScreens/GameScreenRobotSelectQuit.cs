using TMPro;

public class GameScreenRobotSelectQuit : GameScreen
{
    public TextMeshProUGUI WarningText;

    public void SetWarningText(string text)
    {
        WarningText.text = text;
    }
}