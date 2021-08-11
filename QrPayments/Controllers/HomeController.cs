using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace QrPayments.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Public home page.
        /// No authorization required. User doesn't need to login to see this.
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Login action.
        /// No authentication specific code. Just adding the <see cref="AuthorizeAttribute"/>
        /// will trigger authentication if necessary.
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Login()
        {
            /*
            var user = User;
            var id = user.FindFirst(ClaimTypes.NameIdentifier);
            var givenName = user.FindFirst(ClaimTypes.GivenName);
            var surame = user.FindFirst(ClaimTypes.Surname);
            if (bool.Parse(user.FindFirst("email_verified").Value))
            {
                var emailAddress = user.FindFirstValue(ClaimTypes.Email);
            }

            var pictureUrl = user.FindFirst("picture").Value;
            */

            return View();
        }

        /// <summary>
        /// Logout action.
        /// Does nothing if the user is not logged in.
        /// </summary>
        public async Task<IActionResult> Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            return View();
        }

        
        /*/// <summary>
        /// Fetches and shows the Google OAuth2 tokens that are currently active for the logged in user.
        /// Specifying the <see cref="AuthorizeAttribute"/> will guarantee that the code executes only if the
        /// user is authenticated. Once the user is authenticated the tokens are stored locally, in a cookie,
        /// and we can inspect them.
        /// </summary>
        [Authorize]
        public async Task<IActionResult> ShowTokens()
        {
            // The user is already authenticated, so this call won't trigger authentication.
            // But it allows us to access the AuthenticateResult object that we can inspect
            // to obtain token related values.
            AuthenticateResult auth = await HttpContext.AuthenticateAsync();
            string idToken = auth.Properties.GetTokenValue(OpenIdConnectParameterNames.IdToken);
            string idTokenValid, idTokenIssued, idTokenExpires;
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
                idTokenValid = "true";
                idTokenIssued = new DateTime(1970, 1, 1).AddSeconds(payload.IssuedAtTimeSeconds.Value).ToString();
                idTokenExpires = new DateTime(1970, 1, 1).AddSeconds(payload.ExpirationTimeSeconds.Value).ToString();
            }
            catch (Exception e)
            {
                idTokenValid = $"false: {e.Message}";
                idTokenIssued = "invalid";
                idTokenExpires = "invalid";
            }
            string accessToken = auth.Properties.GetTokenValue(OpenIdConnectParameterNames.AccessToken);
            string refreshToken = auth.Properties.GetTokenValue(OpenIdConnectParameterNames.RefreshToken);
            string accessTokenExpiresAt = auth.Properties.GetTokenValue("expires_at");
            string cookieIssuedUtc = auth.Properties.IssuedUtc?.ToString() ?? "<missing>";
            string cookieExpiresUtc = auth.Properties.ExpiresUtc?.ToString() ?? "<missing>";

            return View(new []
            {
                $"Id Token: '{idToken}'",
                $"Id Token valid: {idTokenValid}",
                $"Id Token Issued UTC: '{idTokenIssued}'",
                $"Id Token Expires UTC: '{idTokenExpires}'",
                $"Access Token: '{accessToken}'",
                $"Refresh Token: '{refreshToken}'",
                $"Access token expires at: '{accessTokenExpiresAt}'",
                $"Cookie Issued UTC: '{cookieIssuedUtc}'",
                $"Cookie Expires UTC: '{cookieExpiresUtc}'",
            });
        }*/
    }
}
