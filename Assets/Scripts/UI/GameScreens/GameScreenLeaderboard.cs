using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameScreenLeaderboard : GameScreen
{
    public GameObject LeaderboardEntryPrefab;
    
    public Sprite BackgroundImage;
    public Sprite ActiveTabSprite;
    public Sprite InactiveTabSprite;
    public Color InactiveColor;
    public GameObject LeaderboardEntriesParent;
    public Image FreeTab;
    public TextMeshProUGUI FreeTabText;
    public Image Nethertab;
    public TextMeshProUGUI NetherTabText;

    public void AddEntry(LeaderboardEntry leaderboardEntry)
    {
        GameObject entry = GameObject.Instantiate(LeaderboardEntryPrefab, LeaderboardEntriesParent.transform);
        entry.GetComponent<LeaderboardEntryController>().SetText(leaderboardEntry);
    }

    public void ClearEntries()
    {
        while (LeaderboardEntriesParent.transform.childCount > 0)
        {
            Destroy(LeaderboardEntriesParent.transform.GetChild(0).gameObject);
        }
    }

    public void ActivateFreeTab()
    {
        FreeTab.sprite = ActiveTabSprite;
        Nethertab.sprite = InactiveTabSprite;
        FreeTabText.color = Color.white;
        NetherTabText.color = InactiveColor;
    }

    public void ActivateNetherTab()
    {
        Nethertab.sprite = ActiveTabSprite;
        FreeTab.sprite = InactiveTabSprite;
        NetherTabText.color = Color.white;
        FreeTabText.color = InactiveColor;
    }

}
