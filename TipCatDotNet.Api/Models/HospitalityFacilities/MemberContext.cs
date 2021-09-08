namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public record MemberContext
    {
        public MemberContext(int id, string? email)
        {
            Id = id;
            Email = email;
        }


        public int Id { get; init; }
        public string? Email { get; init; }
    }
}
