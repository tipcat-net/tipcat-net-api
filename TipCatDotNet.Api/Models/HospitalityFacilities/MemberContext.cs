namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public record MemberContext
    {
        public MemberContext(int id, string identityHash, int? accountId, string? email)
        {
            Id = id;
            AccountId = accountId;
            Email = email;
            IdentityHash = identityHash;
        }


        public static MemberContext CreateEmpty() => new(0, string.Empty, null, null);


        public int Id { get; init; }
        public string IdentityHash { get; init; }
        public int? AccountId { get; init; }
        public string? Email { get; init; }
    }
}
