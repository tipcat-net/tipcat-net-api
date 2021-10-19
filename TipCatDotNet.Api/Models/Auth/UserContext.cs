namespace TipCatDotNet.Api.Models.Auth
{
    public readonly struct UserContext
    {
        public UserContext(string? givenName, string? surname, string? email)
        {
            GivenName = givenName;
            Surname = surname;
            Email = email;
        }


        public string? Email { get; }
        public string? GivenName { get; }
        public string? Surname { get; }
    }
}