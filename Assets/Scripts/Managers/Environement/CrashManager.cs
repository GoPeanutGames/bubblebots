using UnityEngine.CrashReportHandler;

public class CrashManager : MonoSingleton<CrashManager>
{
    public void SetCustomCrashKey(CrashTypes crashType, string data)
    {
        CrashReportHandler.SetUserMetadata(crashType.ToString(), data);
    }
}