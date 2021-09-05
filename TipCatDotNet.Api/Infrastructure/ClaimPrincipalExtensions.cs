using System.Linq;
using System.Security.Claims;

namespace TipCatDotNet.Api.Infrastructure
{
    public static class ClaimPrincipalExtensions
    {
        public static string? GetClaimValue(this ClaimsPrincipal principal, string claimType)
            => principal.Claims
                .SingleOrDefault(c => c.Type == claimType)
                ?.Value;


        public static string? GetId(this ClaimsPrincipal principal) 
            => principal.GetClaimValue(ClaimTypes.NameIdentifier);
    }
}
