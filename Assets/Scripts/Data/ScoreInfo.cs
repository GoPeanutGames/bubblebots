using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreInfo
{
    public long Score { get; private set; }
    public string UserFullName { get; private set; }
    public string Country { get; private set; }
    public int Rank { get; private set; }
    public bool Us { get; private set; }

    public ScoreInfo(long score, string userFullName, string country, int rank, bool us)
    {
        Score = score;
        UserFullName = userFullName;
        Country = country;
        Rank = rank;
        Us = us;
    }
}

