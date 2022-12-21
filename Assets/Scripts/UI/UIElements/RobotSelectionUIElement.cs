using UnityEngine;
using UnityEngine.UI;

public class RobotSelectionUIElement : MonoBehaviour
{
    [SerializeField] private Image robotImage;
    [SerializeField] private Button button;
    [SerializaField] public Image labelImage;
    [SerializaField] public TMPro.TextMeshProUGUI labelName;

    private int robotId;

    public void Setup(Sprite sprite, Sprite label, string robotName, int id, System.Action<int> callback)
    {
        robotId = id;
        robotImage.sprite = sprite;
        labelImage.sprite = label;
        labelName.text = robotName.ToUpper();
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
