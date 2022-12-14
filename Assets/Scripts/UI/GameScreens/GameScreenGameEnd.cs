public class GameScreenGameEnd : GameScreen
{
    [SerializaField] public TMPro.TextMeshProUGUI score;

    public void SetScore(string scoreText)
    {
        score.text = scoreText;
    }
}