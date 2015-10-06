using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Twitter;
using Owin;
using SenkaKichi.DbModels;
using SenkaKichi.Models;
using SenkaKichi.OAuthApi.Twitter;
using SenkaKichi.OAuthApi.Facebook;
using SenkaKichi.OAuthApi.Google;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Facebook;

namespace SenkaKichi
{
    public class IdentityConfig
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public static void ConfigureAuth(IAppBuilder app) {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/"),
                CookieSecure = CookieSecureOption.Always
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            app.UseTwitterAuthentication(new TwitterAuthenticationOptions {
                ConsumerKey = TwitterApiManager.ConsumerKey,
                ConsumerSecret = TwitterApiManager.ConsumerSecret,
                Provider = new TwitterAuthenticationProvider {
                    OnAuthenticated = (context) =>
                    {
                        context.Identity.AddClaims(new[] {
                            new Claim("urn:accesstoken", context.AccessToken, ClaimValueTypes.String, "Twitter"),
                            new Claim("urn:accesstokensecret", context.AccessTokenSecret, ClaimValueTypes.String, "Twitter")
                        });
                        return Task.FromResult(0);
                    }
                }
            });

            app.UseFacebookAuthentication(new FacebookAuthenticationOptions {
                AppId = FacebookApiManager.AppID,
                AppSecret = FacebookApiManager.AppSecret,
                Provider = new FacebookAuthenticationProvider {
                    OnAuthenticated = (context) =>
                    {
                        context.Identity.AddClaims(new[] {
                            new Claim("urn:accesstoken", context.AccessToken, ClaimValueTypes.String, "Facebook"),
                            new Claim("urn:accesstokensecret", "", ClaimValueTypes.String, "Facebook")
                        });
                        return Task.FromResult(0);
                    }
                }
            });

            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions {
                ClientId = GoogleApiManager.ClientID,
                ClientSecret = GoogleApiManager.ClientSecret
            });

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");
        }
    }
}
