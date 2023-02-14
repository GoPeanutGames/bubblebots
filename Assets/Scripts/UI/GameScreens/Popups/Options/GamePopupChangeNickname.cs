using TMPro;

public class GamePopupChangeNickname : GameScreenAnimatedEntryExit
{
    public TMP_InputField changeNicknameText;
    public TextMeshProUGUI errorText;

    private void Start()
    {
        errorText.text = "";
    }

    public string GetNicknameText()
    {
        return changeNicknameText.text.Trim();
    }

    public void SetNicknameText(string nickname)
    {
        changeNicknameText.text = nickname;
    }

    public void SetNickDuplicatedError()
    {
        errorText.text = "Username already exists!";
    }
}