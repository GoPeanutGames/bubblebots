using UnityEngine;
using UnityEngine.UI;

public class RobotSelectionUIElement : MonoBehaviour
{
    [SerializeField] private Image robotImage;
    [SerializeField] private Button button;

    private int robotId;

    public void Setup(Sprite sprite, int id, System.Action<int> callback)
    {
        robotId = id;
        robotImage.sprite = sprite;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => { callback(robotId); });
    }
    public int GetId()
    {
        return robotId;
    }

    public void DisableButton()
    {
        button.interactable = false;
    }

    public void EnableButton()
    {
        button.interactable = true;
    }
}
