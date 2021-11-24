namespace TipCatDotNet.Api.Models.Company
{
    // TODO: make an endpoint
    public readonly struct CompanyInfo
    {
        public CompanyInfo(string name, string country, string city, string address, string postalBox, string tradeLicenseNumber)
        {
            Name = name;
            Country = country;
            City = city;
            Address = address;
            PostalBox = postalBox;
            TradeLicenseNumber = tradeLicenseNumber;
        }


        public string Address { get; }
        public string City { get; }
        public string Country { get; }
        public string Name { get; }
        public string PostalBox { get; }
        public string TradeLicenseNumber { get; }
    }
}
