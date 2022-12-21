public class GameScreenLevelComplete : GameScreen
{
    [SerializaField] public TMPro.TextMeshProUGUI message;
    [SerializaField] public TMPro.TextMeshProUGUI buttonText;

    public void SetMessage(string messageText)
    {
        message.text = messageText;
    }

    public void SetButtonText(string _buttonText)
    {
        buttonText.text = _buttonText;
    }
}