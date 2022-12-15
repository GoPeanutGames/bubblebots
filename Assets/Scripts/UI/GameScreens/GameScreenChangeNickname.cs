using TMPro;

public class GameScreenChangeNickname : GameScreen
{
    public TMP_InputField changeNicknameText;

    public string GetNicknameText()
    {
        return changeNicknameText.text.Trim();
    }

    public void SetNicknameText(string nickname)
    {
        changeNicknameText.text = nickname;
    }
}