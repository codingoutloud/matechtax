using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using MATechTaxWebSite.Models;
using System.Configuration;

namespace MATechTaxWebSite
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            var twitterOAuthConsumerKey = ConfigurationManager.AppSettings["TwitterOAuthConsumerKey"];
            var twitterOAuthConsumerSecret = ConfigurationManager.AppSettings["TwitterOAuthConsumerSecret"];
            OAuthWebSecurity.RegisterTwitterClient(
                consumerKey: twitterOAuthConsumerKey,
                consumerSecret: twitterOAuthConsumerSecret);

            var twitterOAuthAccessToken = ConfigurationManager.AppSettings["TwitterOAuthAccessToken"];
            var twitterOAuthAccessTokenSecret = ConfigurationManager.AppSettings["TwitterOAuthAccessTokenSecret"];

            var tweeter = new TweetSharp.TwitterService(twitterOAuthConsumerKey, twitterOAuthConsumerSecret);
            tweeter.AuthenticateWith(twitterOAuthAccessToken, twitterOAuthAccessTokenSecret);
            var status = "The DOR FAQ has been updated: http://matechtax.azurewebsites.net #MATechTax";
            tweeter.SendTweet(new TweetSharp.SendTweetOptions { Status = status });

            //OAuthWebSecurity.RegisterFacebookClient(
            //    appId: "",
            //    appSecret: "");

            //OAuthWebSecurity.RegisterGoogleClient();
        }
    }
}
