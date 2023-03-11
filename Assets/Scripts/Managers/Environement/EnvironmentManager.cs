using BubbleBots.Environment;
using UnityEngine;

public class EnvironmentManager : MonoSingleton<EnvironmentManager>
{
    [SerializeField] private bool Development;
    [SerializeField] private bool Production;
    [SerializeField] private bool Rhym;
    [SerializeField] private EnvironmentSpec DevelopmentEnvironment;
    [SerializeField] private EnvironmentSpec ProductionEnvironment;

    private EnvironmentSpec currentEnvironment;

    protected override void Awake()
    {
        base.Awake();
        currentEnvironment = DevelopmentEnvironment;
        if (Production && !Development)
        {
            currentEnvironment = ProductionEnvironment;
        }
        else if (!Production && !Development)
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

    public string GetCurrentPublicKey()
    {
        return string.Join("\n", currentEnvironment.publicKey);
    }

    public bool IsRhym()
    {
        return Rhym;
    }
    
    public bool IsDevelopment()
    {
        return Development;
    }
}
