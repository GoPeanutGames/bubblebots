using System;
using System.Collections.Generic;

namespace BubbleBots.Server.Referral
{
    public enum ReferralAPI { IsRedeemedPlayer }



    [Serializable]
    public class ReferralIsRedeemedData
    {
        public string signature;
        public string address;
    }

    [Serializable]
    public class ReferralIsRedeemedResponse
    {
        public bool exists;
    }

}