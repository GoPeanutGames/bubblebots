using UnityEngine.UI;

public class GamePopupDeleteAccount : GameScreen
{
    public Toggle acknowledgeToggle;
    public Button proceedButton;

    private void Start()
    {
        acknowledgeToggle.onValueChanged.AddListener(ToggleValueChanged);
        acknowledgeToggle.isOn = false;
    }

    private void ToggleValueChanged(bool val)
    {
        proceedButton.interactable = val;
    }
}
