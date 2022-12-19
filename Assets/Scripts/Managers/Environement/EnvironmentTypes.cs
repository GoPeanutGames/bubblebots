using System;
using System.Collections.Generic;

namespace BubbleBots.Environment
{
    [Serializable]
    public class EnvironmentSpec
    {
        public string serverUrl;
        public string unityEnvironmentName;
        public List<string> publicKey;
    }
}