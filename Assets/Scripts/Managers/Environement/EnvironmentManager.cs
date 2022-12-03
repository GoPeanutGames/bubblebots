using BubbleBots.Environment;
using UnityEngine;

public class EnvironmentManager : MonoSingleton<EnvironmentManager>
{
    [SerializeField] private bool Development;
    [SerializeField] private bool Production;
    [SerializeField] private bool Community;
    [SerializeField] private EnvironmentSpec DevelopmentEnvironment;
    [SerializeField] private EnvironmentSpec ProductionEnvironment;
    [SerializeField] private EnvironmentSpec CommunityEnvironment;

    private EnvironmentSpec currentEnvironment;

    protected override void Awake()
    {
        base.Awake();
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
}
