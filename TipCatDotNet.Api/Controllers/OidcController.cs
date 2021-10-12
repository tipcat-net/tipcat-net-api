using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using TipCatDotNet.Api.Models.Auth;
using TipCatDotNet.Api.Services.Auth;

namespace TipCatDotNet.Api.Controllers
{
    [AllowAnonymous]
    [Route(".well-known")]
    public class OidcController : BaseController
    {
        public OidcController(ICertificateService certificateService)
        {
            _credentials = certificateService.BuildSigningCredentials().GetAwaiter().GetResult();
        }


        [HttpGet]
        [Route("openid-configuration", Name = "OIDCMetadata")]
        public ActionResult Metadata()
        {
            return Content(JsonConvert.SerializeObject(new OidcModel
            {
                // Sample: The issuer name is the application root path
                Issuer = $"{Request.Scheme}://{Request.Host}{Request.PathBase.Value}/",

                // Sample: Include the absolute URL to JWKs endpoint
                JwksUri = Url.Link("JWKS", null),

                // Sample: Include the supported signing algorithms
                IdTokenSigningAlgValuesSupported = new[] { _credentials.Algorithm },
            }), "application/json");
        }


        [HttpGet]
        [Route("keys", Name = "JWKS")]
        public ActionResult JwksDocument()
        {
            return Content(JsonConvert.SerializeObject(new JwksModel
            {
                Keys = new[] { JwksKeyModel.FromSigningCredentials(_credentials) }
            }), "application/json");
        }


        private readonly X509SigningCredentials _credentials;
    }
}
