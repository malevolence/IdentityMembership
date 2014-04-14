using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using WebApplicationMvc.Helpers;

namespace WebApplicationMvc.Models.Identity
{
	public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
	{
		private readonly string _publicClientId;

		public ApplicationOAuthProvider(string publicClientId)
		{
			if (string.IsNullOrEmpty(publicClientId))
			{
				throw new ArgumentNullException("publicClientId");
			}

			_publicClientId = publicClientId;
		}

		public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
		{
			var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
			var user = await userManager.FindByEmailAsync(context.UserName);
			if (user == null)
			{
				context.SetError("invalid_grant", "No user with that email address was found.");
				return;
			}
			else
			{
				var signInHelper = new SignInHelper(userManager, HttpContext.Current.GetOwinContext().Authentication);
				var result = await signInHelper.PasswordSignIn(context.UserName, context.Password, false);
				if (result == SignInStatus.Success)
				{
					ClaimsIdentity oAuthIdentity = await userManager.CreateIdentityAsync(user, context.Options.AuthenticationType);
					ClaimsIdentity cookiesIdentity = await userManager.CreateIdentityAsync(user, CookieAuthenticationDefaults.AuthenticationType);
					AuthenticationProperties properties = CreateProperties(user.UserName);
					AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);
					context.Validated(ticket);
					context.Request.Context.Authentication.SignIn(cookiesIdentity);
				}
				else
				{
					context.SetError("invalid_grant", "Unable to sign in user with these credentials.");
					return;
				}
			}
		}

		public override Task TokenEndpoint(OAuthTokenEndpointContext context)
		{
			foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
			{
				context.AdditionalResponseParameters.Add(property.Key, property.Value);
			}

			return Task.FromResult<object>(null);
		}

		public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
		{
			if (context.ClientId == null)
			{
				context.Validated();
			}

			return Task.FromResult<object>(null);
		}

		public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
		{
			if (context.ClientId == _publicClientId)
			{
				Uri expectedRootUri = new Uri(context.Request.Uri, "/");
				if (expectedRootUri.AbsoluteUri == context.RedirectUri)
				{
					context.Validated();
				}
			}

			return Task.FromResult<object>(null);
		}

		public static AuthenticationProperties CreateProperties(string userName)
		{
			IDictionary<string, string> data = new Dictionary<string, string>
			{
				{ "userName", userName }
			};

			return new AuthenticationProperties(data);
		}
	}
}