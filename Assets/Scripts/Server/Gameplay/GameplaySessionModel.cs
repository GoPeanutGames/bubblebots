using System;

namespace BubbleBots.Server.Gameplay
{
    public enum GameplaySessionAPI { Start, Update, End };

    [Serializable]
    public class GameplaySessionResult
    {
        public string sessionId;
    }

    [Serializable]
    public class GameplaySessionStartData
    {
        public string signature;
        public string address;
        public string timezone;
        public string mode;
        public int level;
        public string startTime;
    }

    [Serializable]
    public class GameplaySessionUpdateData
    {
        public string sessionId;
        public int level;
        public int score;
        public int kills;
        public bool specialBurst;
    }

    [Serializable]
    public class GameplaySessionUpdateDataResponse
    {
        public string sessionId;
        public int level;
        public int score;
        public int kills;
        public bool specialBurst;
        public int bubbles;
    }

    [Serializable]
    public class GameplaySessionEndData
    {
        public string sessionId;
        public int score;
        public string endTime;
    }
}
