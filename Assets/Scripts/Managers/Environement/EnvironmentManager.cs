using BubbleBots.Environment;
using UnityEngine;

public class EnvironmentManager : MonoSingleton<EnvironmentManager>
{
    /*TODO: config for:
     ========DEV
     BundleId: com.peanutgames.minibotsdevnew
     LastBundleVersion: 26
     GOOGLE PLAY GAMES:
     GPG Setup ID (android): 916173128763-0b6c97bkn7ck2120nlsuro5k0vvne1fn.apps.googleusercontent.com
     GPG Client ID: 916173128763-tqei703s3fab014kl49defmbnl73k88n.apps.googleusercontent.com
     GPG Client secret: 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
     APPLE:
     APP ID: 1670620983
     REVENUE CAT:
     Api key Google: goog_jPpsekNkdZvSzPwJcWLkSvLRFof
     Api key Apple: appl_sHJjOuUTDbFSdiXJMPAPiAiMnTY
     =========PROD
     BundleId: com.peanutgames.minibots.prod
     LastBundleversio: 2
     GOOGLE PLAY GAMES: 
     GPG Setup ID (android): 289220171644-gqbpa74u8g63hs3fh5cuj20c9qf0rjtp.apps.googleusercontent.com 
     GPG Client ID: 289220171644-b5e3j0mdhhc6ov1beb46nh8qjel9ils1.apps.googleusercontent.com
     GPG Client secret: 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
     APPLE: 
     APP ID: 289220171644
     REVENUE CAT:
     Api key Google: goog_MXLQDoEZTrocHYukgyiRfmiCfaX
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
