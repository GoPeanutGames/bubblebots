using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Analytics;
using Unity.Services.Core.Environments;
using UnityEngine;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance;

    private string currentWalletAddress;
    private int currentLevelStarted;

    private void Awake()
    {
        Instance = this;
    }

    private void SendLoginEvent()
    {
        AnalyticsService.Instance.CustomData("Login", new Dictionary<string, object>
        {
            {"wallet_address", currentWalletAddress }
        });
    }

    private async void InitAnalyticsPrivately()
    {
        try
        {
            InitializationOptions options = new InitializationOptions();
            options.SetAnalyticsUserId(currentWalletAddress);
            options.SetEnvironmentName(EnvironmentManager.Instance.GetUnityEnvironmentName());
            await UnityServices.InitializeAsync(options);
            List<string> consentIdentifiers = await AnalyticsService.Instance.CheckForRequiredConsents();
            SendLoginEvent();
        }
        catch (ConsentCheckException)
        {
            // Something went wrong when checking the GeoIP, check the e.Reason and handle appropriately.
        }
    }

    public void InitAnalyticsWithWallet(string walletAddress)
    {
        currentWalletAddress = walletAddress;
        InitAnalyticsPrivately();
    }

    public void InitAnalyticsGuest()
    {
        currentWalletAddress = "guest-id";
        InitAnalyticsPrivately();
    }

    public void SendPlayEvent(int levelNumber)
    {
        currentLevelStarted = levelNumber;
        AnalyticsService.Instance.CustomData("StartLevel", new Dictionary<string, object>
        {
            {"wallet_address", currentWalletAddress },
            {"game_level", levelNumber }
        });
    }

    public void SendLevelEvent()
    {
        int score = UserManager.Instance.GetPlayerScore();
        AnalyticsService.Instance.CustomData("EndLevel", new Dictionary<string, object>
        {
            {"wallet_address", currentWalletAddress },
            {"game_level", currentLevelStarted },
            {"score", score }
        });
    }

    public void SendRobotKillEvent(int robotsKilled)
    {
        AnalyticsService.Instance.CustomData("KillRobot", new Dictionary<string, object>
        {
            {"wallet_address", currentWalletAddress },
            {"game_level", currentLevelStarted },
            {"total_kills", robotsKilled }
        });
    }
}
