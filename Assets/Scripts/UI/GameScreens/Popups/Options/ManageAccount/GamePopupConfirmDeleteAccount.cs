using TMPro;

public class GamePopupConfirmDeleteAccount : GameScreen
{
    public TMP_InputField codeInputFieldConfirmDelete;
    
    public string GetCodeConfirmDelete()
    {
        return codeInputFieldConfirmDelete.text;
    }
}
