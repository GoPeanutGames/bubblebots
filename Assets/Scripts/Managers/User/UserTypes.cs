using System;
using UnityEngine;

namespace BubbleBots.User
{
    public enum PrefsKey { Nickname, WalletAddress, SessionToken, Signature, Hints, Avatar }
    public enum PrefsKeyToDelete {Rank}

    [Serializable]
    public class NFTImage
    {
        public string url;
        public int tokenId;
        public bool loaded;
        public Sprite sprite;
    }

    [Serializable]
    public class User
    {
        public string UserName;
        public string WalletAddress;
        public string SessionToken;
        public string Signature;
        public bool Hints;
        public AvatarInformation Avatar;
    }

    [Serializable]
    public class AvatarInformation
    {
        public int id;
        public bool isNft;
    }
}