namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public record MemberContext
    {
        public MemberContext(int id, int? accountId, string? email)
        {
            Id = id;
            AccountId = accountId;
            Email = email;
        }


        public int Id { get; init; }
        public int? AccountId { get; }
        public string? Email { get; init; }
    }
}
