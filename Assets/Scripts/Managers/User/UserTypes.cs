using System;

namespace BubbleBots.User
{
    public enum PrefsKey { Nickname, WalletAddress, SessionToken, Signature, Hints, Avatar }
    public enum PrefsKeyToDelete {Rank}

    [Serializable]
    public class User
    {
        public string UserName;
        public string WalletAddress;
        public string SessionToken;
        public string Signature;
        public bool Hints;
        public int Avatar;
    }
}