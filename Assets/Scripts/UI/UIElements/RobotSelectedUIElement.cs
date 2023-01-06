using UnityEngine;
using UnityEngine.UI;

public class RobotSelectedUIElement : MonoBehaviour
{
    [SerializeField] private Image frame;
    [SerializeField] private Image background;
    [SerializeField] private Image badge;
    [SerializeField] private Image robotImage;
    [SerializeField] private GameObject cross;
    [SerializeField] private Button button;
    [SerializeField] private Sprite emptyFrame;


    private BubbleBots.Data.BubbleBotData bubbleBotData;

    public void ShowImage(BubbleBots.Data.BubbleBotData _bubbleBotData, System.Action<int> callback)
    {
        bubbleBotData = _bubbleBotData;
        frame.sprite = bubbleBotData.frameSprite;
        background.gameObject.SetActive(true);
        background.sprite = bubbleBotData.bgSprite;
        badge.gameObject.SetActive(true);
        badge.sprite = bubbleBotData.badgeSprite;
        robotImage.gameObject.SetActive(true);
        robotImage.sprite = bubbleBotData.robotSprite;
        cross.SetActive(true);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => { callback(bubbleBotData.id); });
    }

    public void HideImage()
    {
        cross.SetActive(false);
        frame.sprite = emptyFrame;
        background.gameObject.SetActive(false);
        badge.gameObject.SetActive(false);
        robotImage.gameObject.SetActive(false);
        button.onClick.RemoveAllListeners();
    }
}
