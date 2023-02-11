using BubbleBots.Environment;
using UnityEngine;

public class EnvironmentManager : MonoSingleton<EnvironmentManager>
{
    /*TODO: config for:
     ========DEV
     BundleId: com.peanutgames.minibotsdevnew
     LastBundleVersion: 17
     GOOGLE PLAY GAMES:
     GPG Client ID: 916173128763-tqei703s3fab014kl49defmbnl73k88n.apps.googleusercontent.com
     GPG Client secret: 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
     APPLE:
     APP ID: 1670620983
     REVENUE CAT:
     Api key Google: goog_jPpsekNkdZvSzPwJcWLkSvLRFof
     Api key Apple: appl_sHJjOuUTDbFSdiXJMPAPiAiMnTY
     =========PROD
     BundleId: com.peanutgames.minibots.prod
     LastBundleversio: 1
     GOOGLE PLAY GAMES:
     GPG Client ID:
     GPG Client secret:
     APPLE:
     APP ID:
     REVENUE CAT:
     Api key Google:
     Api key Apple:
     */
    
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
