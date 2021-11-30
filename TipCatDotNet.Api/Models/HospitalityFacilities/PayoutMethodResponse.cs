using System.Collections.Generic;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public class PayoutMethodResponse
    {
        public PayoutMethodResponse(List<string> externalAccounts)
        {
            ExternalAccounts = externalAccounts;
        }


        public List<string> ExternalAccounts { get; }
    }
}