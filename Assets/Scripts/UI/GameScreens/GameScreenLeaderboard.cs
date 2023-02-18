using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameScreenLeaderboard : GameScreenAnimatedEntryExit
{
    public GameObject LeaderboardEntryPrefab;
    
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
        foreach (Transform child in LeaderboardEntriesParent.transform)
        {
            Destroy(child.gameObject);
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
