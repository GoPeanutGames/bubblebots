using CodeStage.AntiCheat.ObscuredTypes;
using System;

namespace BubbleBots.User
{
    public enum PrefsKey { Nickname, WalletAddress, SessionToken, Rank, Signature }

    [Serializable]
    public class User
    {
        public string UserName;
        public string WalletAddress;
        public ObscuredInt Score;
        public ObscuredInt Rank;
        public string SessionToken;
        public string Signature;
    }
}