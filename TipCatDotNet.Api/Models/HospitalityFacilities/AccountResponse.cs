namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct AccountResponse
    {
        public AccountResponse(int id, string name, string commercialName, string address, string email, string phone, bool isActive)
        {
            Id = id;
            Address = address;
            CommercialName = commercialName;
            Email = email;
            IsActive = isActive;
            Name = name;
            Phone = phone;
        }


        public int Id { get; }
        public string Address { get; }
        public string CommercialName { get; }
        public string Email { get; }
        public bool IsActive { get; }
        public string Name { get; }
        public string Phone { get; }
    }
}
