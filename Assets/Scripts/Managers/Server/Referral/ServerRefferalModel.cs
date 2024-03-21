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


}