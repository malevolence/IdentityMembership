﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Microsoft.AspNet.Identity.Owin;
using WebApplicationMvc.Models;
using WebApplicationMvc.Models.Identity;

namespace CorsApi
{
    public partial class Startup
    {
        static Startup()
        {
            PublicClientId = "self";

			OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                Provider = new ApplicationOAuthProvider(PublicClientId),
                AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
                AllowInsecureHttp = true
            };
        }

        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static string PublicClientId { get; private set; }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
			// Configure db context, role manager and user manager to use single instance per request
			app.CreatePerOwinContext<ApplicationDbContext>(ApplicationDbContext.Create);
			app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);
			app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);

			// Enable the application to use a cookie to store information for the signed in user
			app.UseCookieAuthentication(new CookieAuthenticationOptions
			{
				AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
				LoginPath = new PathString("/Account/Login"),
				Provider = new CookieAuthenticationProvider
				{
					OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser, Guid>(
						TimeSpan.FromMinutes(20),
						(manager, user) => user.GenerateUserIdentityAsync(manager),
						(id) => Guid.Parse(id.GetUserId())
					)
				}
			});

            // Enable the application to use bearer tokens to authenticate users
            app.UseOAuthBearerTokens(OAuthOptions);

            // Enable CORS
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //    consumerKey: "",
            //    consumerSecret: "");

            //app.UseFacebookAuthentication(
            //    appId: "",
            //    appSecret: "");

            //app.UseGoogleAuthentication();
        }
    }
}
