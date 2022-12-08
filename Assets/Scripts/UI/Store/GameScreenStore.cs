using System.Collections.Generic;
using BubbleBots.Store;

public class GameScreenStore : GameScreen
{
    public List<GameStoreTab> StoreTabs;

    private void DeactivateAllTabs()
    {
        foreach (GameStoreTab storeTab in StoreTabs)
        {
            storeTab.DeactivateTab();
        }
    }
    
    public void ActivateTab(StoreTabs tab)
    {
        DeactivateAllTabs();
        StoreTabs[(int)tab].ActivateTab();
    }
}