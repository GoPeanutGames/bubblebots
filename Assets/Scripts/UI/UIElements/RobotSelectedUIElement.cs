using UnityEngine;
using UnityEngine.UI;

public class RobotSelectedUIElement : MonoBehaviour
{
    [SerializeField] private GameObject avatar;
    [SerializeField] private GameObject frame;
    [SerializeField] private GameObject plus;
    [SerializeField] private GameObject cross;
    [SerializeField] private Button button;


    private BubbleBots.Data.BubbleBotData bubbleBotData;

    public void ShowImage(BubbleBots.Data.BubbleBotData _bubbleBotData, System.Action<int> callback)
    {
        bubbleBotData = _bubbleBotData;
        avatar.SetActive(true);
        avatar.GetComponent<Image>().enabled = true;
        avatar.GetComponent<Image>().sprite = bubbleBotData.sprite;
        plus.SetActive(false);
        cross.SetActive(true);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => { callback(bubbleBotData.id); });
    }

    public void HideImage()
    {
        plus.SetActive(true);
        avatar.SetActive(false);
        cross.SetActive(false);
        button.onClick.RemoveAllListeners();
    }
}
