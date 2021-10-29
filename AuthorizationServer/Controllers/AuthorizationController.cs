using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace AuthorizationServer.Controllers
{
    public class AuthorizationController : Controller
    {
        [HttpPost("~/connect/token")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest()
                          ?? throw new InvalidOperationException("The OpenId Connect request cannot be retrieve");

            ClaimsPrincipal claims;

            if (request.IsClientCredentialsGrantType())
            {
                var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId ?? throw new InvalidOperationException());
                claims = new ClaimsPrincipal(identity);
                claims.SetScopes(request.GetScopes());
            }
            else if (request.IsAuthorizationCodeGrantType())
            {
                claims = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            }
            else if (request.IsRefreshTokenGrantType())
            {
                claims = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            }
            else
            {
                throw new InvalidOperationException("The specified grant type is not supported.");
            }

            return SignIn(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Authorize()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                          throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");



            // Retrieve the user principal stored in the authentication cookie.
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);



            // If the user principal can't be extracted, redirect the user to the login page.
            if (!result.Succeeded)
            {
                return Challenge(
                                 authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
                                 properties: new AuthenticationProperties
                                 {
                                     RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                                                                                                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                                 });
            }

            // Create a new claims principal
            var claims = new List<Claim>
            {
                // 'subject' claim which is required
                new Claim(OpenIddictConstants.Claims.Subject, result.Principal.Identity.Name),
                new Claim("some claim", "some value").SetDestinations(OpenIddictConstants.Destinations.AccessToken),
                new Claim(OpenIddictConstants.Claims.Email, "admin@csf.org.tw").SetDestinations(OpenIddictConstants.Destinations.IdentityToken)
            };

            var claimsIdentity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Set requested scopes (this is not done automatically)
            claimsPrincipal.SetScopes(request.GetScopes());

            // Signing in with the OpenIddict authentiction scheme trigger OpenIddict to issue a code (which can be exchanged for an access token)
            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
    }
}
