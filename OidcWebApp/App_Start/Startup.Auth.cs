using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using OidcWebApp_Multiple.Utils;

namespace OidcWebApp_Multiple
{
	public partial class Startup
	{
		/*
        * Configure the OWIN middleware
        */
		public void ConfigureAuth(IAppBuilder app)
		{
			app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
			app.UseCookieAuthentication(new CookieAuthenticationOptions());

			if (Globals.UseB2C)
			{
				ConfigureForB2C(app);
			}
			else
			{
				ConfigureForAAD(app);
			}
		}

		private void ConfigureForAAD(IAppBuilder app)
		{
			app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
			{
				ClientId = AAD_Globals.ClientId,
				Authority = AAD_Globals.Authority,
				PostLogoutRedirectUri = AAD_Globals.RedirectUri,

				Notifications = new OpenIdConnectAuthenticationNotifications()
				{
					// If there is a code in the OpenID Connect response, redeem it for an access token and refresh token, and store those away.
					AuthorizationCodeReceived = (context) =>
					{
						var code = context.Code;

						var appBuilder = ConfidentialClientApplicationBuilder
							.Create(AAD_Globals.ClientId)
							.WithClientSecret(AAD_Globals.ClientSecret)
							.WithRedirectUri(AAD_Globals.RedirectUri)
							.WithTenantId(AAD_Globals.TenantId)
							.Build();

						var authBuilder = appBuilder.AcquireTokenByAuthorizationCode(new string[] { "user.read" }, code);
						var result = authBuilder.ExecuteAsync().Result;

						// The access token can be used to pass to, for example, an API for bearer authentication
						var token = result.AccessToken;

						return Task.FromResult(0);
					}
				}
			});
		}

		private void ConfigureForB2C(IAppBuilder app)
		{
			// Required for Azure webapps, as by default they force TLS 1.2 and this project attempts 1.0
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
			{
				// Generate the metadata address using the tenant and policy information
				MetadataAddress = string.Format(B2C_Globals.WellKnownMetadata, B2C_Globals.Tenant, B2C_Globals.DefaultPolicy),

				// These are standard OpenID Connect parameters, with values pulled from web.config
				ClientId = B2C_Globals.ClientId,
				RedirectUri = B2C_Globals.RedirectUri,
				PostLogoutRedirectUri = B2C_Globals.RedirectUri,

				// Specify the callbacks for each type of notifications
				Notifications = new OpenIdConnectAuthenticationNotifications
				{
					RedirectToIdentityProvider = OnRedirectToIdentityProvider,
					AuthorizationCodeReceived = OnAuthorizationCodeReceived,
					AuthenticationFailed = OnAuthenticationFailed,
				},

				// Specify the claim type that specifies the Name property.
				TokenValidationParameters = new TokenValidationParameters
				{
					NameClaimType = "name",
					ValidateIssuer = false
				},

				// Specify the scope by appending all of the scopes requested into one string (separated by a blank space)
				Scope = $"openid profile offline_access {B2C_Globals.ReadTasksScope} {B2C_Globals.WriteTasksScope}"
			});
		}


		/*
         *  On each call to Azure AD B2C, check if a policy (e.g. the profile edit or password reset policy) has been specified in the OWIN context.
         *  If so, use that policy when making the call. Also, don't request a code (since it won't be needed).
         */
		private Task OnRedirectToIdentityProvider(RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
		{
			var policy = notification.OwinContext.Get<string>("Policy");

			if (!string.IsNullOrEmpty(policy) && !policy.Equals(B2C_Globals.DefaultPolicy))
			{
				notification.ProtocolMessage.Scope = OpenIdConnectScope.OpenId;
				notification.ProtocolMessage.ResponseType = OpenIdConnectResponseType.IdToken;
				notification.ProtocolMessage.IssuerAddress = notification.ProtocolMessage.IssuerAddress.ToLower().Replace(B2C_Globals.DefaultPolicy.ToLower(), policy.ToLower());
			}

			return Task.FromResult(0);
		}

		/*
         * Catch any failures received by the authentication middleware and handle appropriately
         */
		private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
		{
			notification.HandleResponse();

			// Handle the error code that Azure AD B2C throws when trying to reset a password from the login page
			// because password reset is not supported by a "sign-up or sign-in policy"
			if (notification.ProtocolMessage.ErrorDescription != null && notification.ProtocolMessage.ErrorDescription.Contains("AADB2C90118"))
			{
				// If the user clicked the reset password link, redirect to the reset password route
				notification.Response.Redirect("/Account/ResetPassword");
			}
			else if (notification.Exception.Message == "access_denied")
			{
				notification.Response.Redirect("/");
			}
			else
			{
				notification.Response.Redirect("/Home/Error?message=" + notification.Exception.Message);
			}

			return Task.FromResult(0);
		}

		/*
         * Callback function when an authorization code is received
         */
		private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification notification)
		{
			try
			{
				IConfidentialClientApplication confidentialClient = MsalAppBuilder.BuildConfidentialClientApplication(new ClaimsPrincipal(notification.AuthenticationTicket.Identity));

				// Upon successful sign in, get & cache a token using MSAL
				AuthenticationResult result = await confidentialClient.AcquireTokenByAuthorizationCode(B2C_Globals.Scopes, notification.Code).ExecuteAsync();
			}
			catch (Exception ex)
			{
				throw new HttpResponseException(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.BadRequest,
					ReasonPhrase = $"Unable to get authorization code {ex.Message}."
				});
			}
		}
	}
}