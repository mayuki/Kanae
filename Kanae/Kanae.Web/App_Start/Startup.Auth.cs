using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using Kanae.Web.Infrastracture.Extension;
using System.Configuration;

namespace Kanae.Web
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            if (String.Compare(ConfigurationManager.AppSettings["Kanae:UseIntegratedWindowsAuthentication"], "true", true) == 0)
            {
                // 統合Windows認証を使う
                AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.PrimarySid;
            }
            else
            {
                // 外部認証を使う
                // Enable the application to use a cookie to store information for the signed in user
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = DefaultAuthenticationTypes.ExternalCookie,
                    LoginPath = new PathString("/Account/Login")
                });
                // Use a cookie to temporarily store information about a user logging in with a third party login provider
                app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

                var authProviders = (ConfigurationManager.AppSettings["Kanae:AuthenticationProviders"] ?? "")
                                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(x => x.Trim())
                                        .ToList();

                if (authProviders.Contains("Microsoft", StringComparer.OrdinalIgnoreCase))
                {
                    app.UseMicrosoftAccountAuthentication(
                        clientId: ConfigurationManager.AppSettings["Kanae:AuthenticationProvider:Microsoft:ClientId"],
                        clientSecret: ConfigurationManager.AppSettings["Kanae:AuthenticationProvider:Microsoft:ClientSecret"]);
                }
                if (authProviders.Contains("Twitter", StringComparer.OrdinalIgnoreCase))
                {
                    app.UseTwitterAuthentication(
                       consumerKey: ConfigurationManager.AppSettings["Kanae:AuthenticationProvider:Twitter:ConsumerKey"],
                       consumerSecret: ConfigurationManager.AppSettings["Kanae:AuthenticationProvider:Twitter:ConsumerSecret"]);
                }
                if (authProviders.Contains("Facebook", StringComparer.OrdinalIgnoreCase))
                {
                    app.UseFacebookAuthentication(
                       appId: ConfigurationManager.AppSettings["Kanae:AuthenticationProvider:Facebook:AppId"],
                       appSecret: ConfigurationManager.AppSettings["Kanae:AuthenticationProvider:Facebook:AppSecret"]);
                }
                if (authProviders.Contains("Google", StringComparer.OrdinalIgnoreCase))
                {
                    app.UseGoogleAuthentication();
                }
                if (authProviders.Contains("OpenIDConnect", StringComparer.OrdinalIgnoreCase))
                {
                    var authority = ConfigurationManager.AppSettings["Kanae:AuthenticationProvider:OpenIDConnect:Authority"];
                    var caption = ConfigurationManager.AppSettings["Kanae:AuthenticationProvider:OpenIDConnect:Caption"];
                    app.UseOpenIdConnectAuthentication(
                        new OpenIdConnectAuthenticationOptions()
                        {
                            ClientId = ConfigurationManager.AppSettings["Kanae:AuthenticationProvider:OpenIDConnect:ClientId"],
                            Authority = authority,
                            AuthenticationMode = AuthenticationMode.Passive,
                            Caption = String.IsNullOrWhiteSpace(caption)
                                ? String.Format("OpenID Connect ({0})", new Uri(authority).Host)
                                : caption,
                        }
                    );
                }

                AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
            }

            app.UseSetUserRole();
        }
    }
}