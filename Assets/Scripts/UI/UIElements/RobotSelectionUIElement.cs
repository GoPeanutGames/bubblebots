using BubbleBots.Data;
using UnityEngine;
using UnityEngine.UI;

public class RobotSelectionUIElement : MonoBehaviour
{
    
    [SerializeField] private Image bgImage;
    [SerializeField] private Image robotImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image badgeImage;
    [SerializeField] private Button button;
    [SerializaField] public Image labelImage;
    [SerializaField] public TMPro.TextMeshProUGUI labelName;

    private int robotId;

    public void Setup(BubbleBotData botData, System.Action<int> callback)
    {
        robotId = botData.id;
        bgImage.sprite = botData.bgSprite;
        robotImage.sprite = botData.robotSprite;
        frameImage.sprite = botData.frameSprite;
        labelImage.sprite = botData.labelSprite;
        badgeImage.sprite = botData.badgeSprite;
        labelName.text = botData.botName.ToUpper();
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
