using System;

namespace BubbleBots.Server.Player
{
    public enum PlayerAPI { Create, Get, UpdateNickname, Top100 };

    [Serializable]
    public class CreatePlayerData
    {
        public string address;
        public string signature;
    }

    [Serializable]
    public class ChangeUserNameData
    {
        public string address;
        public string nickname;
    }

    [Serializable]
    public class GetPlayerDataResult
    {
        public string nickname;
        public int rank;
    }
}