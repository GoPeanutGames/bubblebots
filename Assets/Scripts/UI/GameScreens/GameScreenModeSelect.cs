using UnityEngine;

public class GameScreenModeSelect : GameScreen
{
    public GameObject toolTipParent;
    public GameObject toolTipFreeMode;
    public GameObject toolTupNetherMode;


    public void ShowToolTipFreeMode()
    {
        toolTipParent.SetActive(true);
        toolTipFreeMode.SetActive(true);
        toolTupNetherMode.SetActive(false);
    }
    public void ShowToolTipNetherMode()
    {
        toolTipParent.SetActive(true);
        toolTipFreeMode.SetActive(false);
        toolTupNetherMode.SetActive(true);
    }

    public void HideToolTips()
    {
        toolTipParent.SetActive(false);
    }
}
