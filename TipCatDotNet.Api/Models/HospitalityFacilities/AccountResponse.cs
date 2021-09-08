namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct AccountResponse
    {
        public AccountResponse(int id, bool isActive)
        {
            Id = id;
            IsActive = isActive;
        }


        public int Id { get; }
        public bool IsActive { get; }
    }
}
