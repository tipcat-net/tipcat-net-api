namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct MemberVerifyEmailRequest
    {
        public MemberVerifyEmailRequest(string code)
        {
            Code = code;
        }

        public string Code { get; }
    }
}