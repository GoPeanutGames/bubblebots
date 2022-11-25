using Bubblebots.Environment;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance;

    [SerializeField] private bool Development;
    [SerializeField] private bool Production;
    [SerializeField] private bool Community;
    [SerializeField] private EnvironmentSpec DevelopmentEnvironment;
    [SerializeField] private EnvironmentSpec ProductionEnvironment;
    [SerializeField] private EnvironmentSpec CommunityEnvironment;

    private EnvironmentSpec currentEnvironment;

    private void Awake()
    {
        Instance = this;
        currentEnvironment = DevelopmentEnvironment;
        if (Production && !Community && !Development)
        {
            currentEnvironment = ProductionEnvironment;
        }
        else if (Community && !Production && !Development)
        {
            currentEnvironment = DevelopmentEnvironment;
        }
    }

    public string GetServerUrl()
    {
        return currentEnvironment.serverUrl;
    }

    public string GetUnityEnvironmentName()
    {
        return currentEnvironment.unityEnvironmentName;
    }

    public string GetSceneName()
    {
        return currentEnvironment.sceneName;
    }

    public bool ShouldChangeRobotImages()
    {
        return Community == false;
    }

    public string GetEncryptPass()
    {
        return currentEnvironment.encryptPass;
    }
}
