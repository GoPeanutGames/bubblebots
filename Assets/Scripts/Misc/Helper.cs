using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helper : MonoBehaviour
{
    public void SetURL(string url)
    {
        if (url.Contains("community"))
        {
            LeaderboardManager.Instance.ServerURL = "https://api-bb-community.peanutgames.com";
        }
        else
        {
            LeaderboardManager.Instance.ServerURL = "https://api-bb.peanutgames.com";
        }
    }

}
