using System;
using System.Collections.Generic;

namespace BubbleBots.Server.Player
{
    public enum PlayerAPI { Create, Get, UpdateNickname, Top100Pro, Top100Free, Wallet, Battlepass, GetOwnedNFTs, SetDefaultNFT };

    [Serializable]
    public class CreatePlayerData
    {
        public string address;
        public string signature;
    }

    [Serializable]
    public class GetPlayerNftsResponse
    {
        public List<NftDataResponse> images;
    }

    [Serializable]
    public class NftDataResponse
    {
        public string name;
        public int tokenId;
    }

    [Serializable]
    public class SetDefaultNftData
    {
        public string address;
        public string signature;
        public int tokenId;
    }

    [Serializable]
    public class ResponseLeaderboardDataEntry
    {
        public int score;
        public string nickname;
        public int rank;
        public string address;
    }

    [Serializable]
    public class GetBattlePassResponse
    {
        public bool exists;
    }
    
    [Serializable]
    public class GetLeaderboardData
    {
        public List<ResponseLeaderboardDataEntry> activities;
    }

    [Serializable]
    public class ChangeUserNameData
    {
        public string signature;
        public string address;
        public string nickname;
    }

    [Serializable]
    public class GetPlayerDataResult
    {
        public string nickname;
        public int rank;
    }

    [Serializable]
    public class GetPlayerWallet
    {
        public int gems;
        public int energy;
        public int bubbles;
    }
}