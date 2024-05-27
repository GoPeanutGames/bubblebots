using System.Collections.Generic;
using UnityEngine.UI;

public class GameScreenBoostersPopup : GameScreenAnimatedEntryExit
{
    public Button button;
    public TMPro.TextMeshProUGUI buttonText;

    public List<BoosterUIElement> elements;

    private int index = 0;
    public bool canUse = false;

    public void Refresh()
    {
        index = 0;
        for (int i = 0; i < elements.Count; i++)
        {
            elements[i].counter.text = UserManager.Instance.GetBoosterCount(elements[i].BoosterId).ToString();
        }
        SelectBooster(0);
    }

    public BoosterId GetSelectedBooster()
    {
        return elements[0].BoosterId;
    }

    public void SelectBooster(int index)
    {
        if (UserManager.Instance.GetBoosterCount(elements[index].BoosterId) <= 0)
        {
            buttonText.text = "BUY";
            canUse = false;
        } 
        else
        {
            buttonText.text = "USE";
            canUse = true;
        }
    }
}
