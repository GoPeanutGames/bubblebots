using CodeStage.AntiCheat.ObscuredTypes;
using System;

namespace Bubblebots.Environment
{
    [Serializable]
    public class EnvironmentSpec
    {
        public string serverUrl;
        public string unityEnvironmentName;
        public string sceneName;
        public ObscuredString encryptPass;
    }
}