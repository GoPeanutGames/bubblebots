using TMPro;

public class GameScreenQuitToMainMenu : GameScreen
{
    public TextMeshProUGUI quitText;

    public void SetQuitText(string text)
    {
        quitText.text = text;
    }
}