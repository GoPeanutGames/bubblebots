using TMPro;
using UnityEngine;

public class LeaderboardEntryController : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nicknameText;
    public TextMeshProUGUI scoreText;

    public void SetText(LeaderboardEntry leaderboardEntry)
    {
        rankText.text = leaderboardEntry.rank;
        nicknameText.text = leaderboardEntry.nickname;
        scoreText.text = leaderboardEntry.score;
    }
}
