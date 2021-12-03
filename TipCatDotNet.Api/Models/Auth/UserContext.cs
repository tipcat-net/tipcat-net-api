namespace TipCatDotNet.Api.Models.Auth;

public readonly struct UserContext
{
    public UserContext(string? givenName, string? surname, string? email)
    {
        GivenName = givenName ?? string.Empty;
        Surname = surname ?? string.Empty;
        Email = email;
    }


    public string? Email { get; }
    public string? GivenName { get; }
    public string? Surname { get; }
}