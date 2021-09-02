namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public class SignInResponse
    {
        public SignInResponse(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
        
        public string AccessToken { get; init; }
        
        public string RefreshToken { get; init; }
    }
}