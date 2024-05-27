using System;
using UnityEngine;

namespace BubbleBots.User
{
    public enum PrefsKey { Nickname, WalletAddress, SessionToken, Signature, Settings, Avatar, CurrentLevel }
    public enum PrefsKeyToDelete {Hints}

    [Serializable]
    public class NFTImage
    {
        public string url;
        public int tokenId;
        public bool loaded;
        public Sprite sprite;
    }

    [Serializable]
    public class Settings
    {
        public bool hints;
        public bool music;
    }

    [Serializable]
    public class User
    {
        public string UserName;
        public string WalletAddress;
        public string SessionToken;
        public string Signature;
        public Settings settings;
        public AvatarInformation Avatar;
    }

    [Serializable]
    public class AvatarInformation
    {
        public int id;
        public bool isNft;
    }
}