public class GameScreenGameEnd : GameScreen
{
    [SerializaField] public TMPro.TextMeshProUGUI message;

    public void SetMessage(string messageText)
    {
        message.text = messageText;
    }

}