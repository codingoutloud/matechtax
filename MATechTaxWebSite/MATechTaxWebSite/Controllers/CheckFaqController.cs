using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ValetKeyPattern.AzureStorage;

namespace MATechTaxWebSite.Controllers
{
    public class CheckFaqController : ApiController
    {
        // GET api/checkfaq
        public HttpResponseMessage Get()
        {
            string httpLastModified = String.Empty;
            try
            {
                httpLastModified = CheckFaq();
            }
            catch (Exception ex)
            {
                var foo = ex;
            }

            return Request.CreateResponse(HttpStatusCode.OK, httpLastModified);
        }

        private string CheckFaq()
        {
            var httpLastModified = String.Empty;
            var client = new HttpClient();
            var faqUrl = ConfigurationManager.AppSettings["DorFaqUrl"];
            var result =
               client.GetAsync(faqUrl).Result;
            var contents = result.Content.ReadAsByteArrayAsync().Result;
            IEnumerable<string> lastModifiedHeaders;
            if (result.Content.Headers.TryGetValues("Last-Modified", out lastModifiedHeaders))
            {
                httpLastModified = lastModifiedHeaders.First();
            }
            var thumbprint = GetThumbprint(contents);
            var blobUrl = ConfigurationManager.AppSettings["BlobValetKeyUrl"];
            var blobUri = BlobContainerValet.GetDestinationPathFromValetKey(blobUrl);

            WriteToBlobIfChanged(blobUri, thumbprint, contents);

            return httpLastModified;
        }

        static string GetThumbprint(byte[] bytes)
        {
            var data = System.Security.Cryptography.SHA1.Create().ComputeHash(bytes);
            return Convert.ToBase64String(data);
        }

        static bool WriteToBlobIfChanged(Uri blobUri, string thumbprint, byte[] contents)
        {
     //       Console.Write("[thumbprint = {0} ... ", thumbprint);

            var valetKeyUrl = ConfigurationManager.AppSettings["BlobValetKeyUrl"];
            var destinationUrl = ConfigurationManager.AppSettings["BlobDestinationUrl"];


            var blob = BlobContainerValet.GetCloudBlockBlob(valetKeyUrl, new Uri(destinationUrl));

            try
            {
                blob.FetchAttributes();
            }
            catch (Microsoft.WindowsAzure.Storage.StorageException ex)
            {
                // if (Enum.GetName(typeof(RequestResult), ex.RequestInformation.HttpStatusCode) == HttpStatusCode.NotFound))
                if (ex.RequestInformation.HttpStatusCode == Convert.ToInt32(HttpStatusCode.NotFound))
                {
                    // write it the first time
                    WriteBlob(blobUri, thumbprint, contents);
                    Console.WriteLine(" (created)");
                    return true;
                }
                else
                {
                    throw;
                }
            }
            var oldthumbprint = blob.Metadata["thumbprint"];

            if (oldthumbprint != thumbprint)
            {
                Console.Write("Change detected: {0}... ", DateTime.Now.ToLongTimeString());

                var snapshot = blob.CreateSnapshot();
                WriteBlob(blobUri, thumbprint, contents);

                TweetThatFaqUpdated();
                return true;
            }
            else
            {
                Console.WriteLine("No change at {0}...", DateTime.Now.ToLongTimeString());

                return false;
            }
        }

        static void TweetThatFaqUpdated()
        {
            var twitterOAuthConsumerKey = ConfigurationManager.AppSettings["TwitterOAuthConsumerKey"];
            var twitterOAuthConsumerSecret = ConfigurationManager.AppSettings["TwitterOAuthConsumerSecret"];
            var twitterOAuthAccessToken = ConfigurationManager.AppSettings["TwitterOAuthAccessToken"];
            var twitterOAuthAccessTokenSecret = ConfigurationManager.AppSettings["TwitterOAuthAccessTokenSecret"];

            var tweeter = new TweetSharp.TwitterService(twitterOAuthConsumerKey, twitterOAuthConsumerSecret);
            tweeter.AuthenticateWith(twitterOAuthAccessToken, twitterOAuthAccessTokenSecret);
            var status = "The DOR FAQ has been updated: http://bit.ly/dorfaqhistory #MATechTax";
            tweeter.SendTweet(new TweetSharp.SendTweetOptions { Status = status });
        }

        static void WriteBlob(Uri blobUri, string thumbprint, byte[] contents)
        {
#if false
         var blob = new CloudBlockBlob(blobUri);
         BlobContainerValet.UploadToBlobContainer(blobUri.AbsoluteUri, path);
#else
            var valetKeyUrl = ConfigurationManager.AppSettings["BlobValetKeyUrl"];
            var destinationUrl = ConfigurationManager.AppSettings["BlobDestinationUrl"];
            var blob = ValetKeyPattern.AzureStorage.BlobContainerValet.GetCloudBlockBlob(valetKeyUrl, new Uri(destinationUrl));
            var memstream = new MemoryStream(contents);
            blob.UploadFromStream(memstream);
            blob.Metadata["thumbprint"] = thumbprint;
            blob.SetMetadata();
            blob.Properties.ContentType = ConfigurationManager.AppSettings["BlobDestinationMimeType"];
            blob.SetProperties();
#endif



            /*
         var uri = Microsoft.WindowsAzure.StorageClient.Protocol.BlobRequest.Get
            (snapshot.Uri,
             0,
             snapshot.SnapshotTime.Value,
             null).Address.AbsoluteUri;
          */
        }
    }
}
