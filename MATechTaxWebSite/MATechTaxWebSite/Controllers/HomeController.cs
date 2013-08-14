using MATechTaxWebSite.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MATechTaxWebSite.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "MA Tech Tax";

            try
            {
                var faqSnapshots = GetFaqSnapshots();
                return View(faqSnapshots);
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.ToString() + " -- {BASE} -> (((" +
                        ex.GetBaseException().ToString() +
                        ")))"
                        ;
                return View(new List<BlobSnapshot>());
            }
        }

        private List<BlobSnapshot> GetFaqSnapshots()
        {
            var faqSnapshots = new List<BlobSnapshot>();
            var valetKeyUrl = ConfigurationManager.AppSettings["BlobValetKeyUrl"];
            var destinationUrl = ConfigurationManager.AppSettings["BlobDestinationUrl"];
            var blob = ValetKeyPattern.AzureStorage.BlobContainerValet.GetCloudBlockBlob(valetKeyUrl, new Uri(destinationUrl));

            FetchMeta(blob);

            var blobOptions = Microsoft.WindowsAzure.Storage.Blob.BlobListingDetails.Snapshots; // | Microsoft.WindowsAzure.Storage.Blob.BlobListingDetails.Metadata;
            foreach (CloudBlockBlob blobItem in blob.Container.ListBlobs(prefix: null, useFlatBlobListing: true, blobListingDetails: blobOptions))
            {
#if true
                var lastModified = String.Empty;
                var hasLastModified = blob.Metadata.TryGetValue("LastModified", out lastModified);
                faqSnapshots.Add(new BlobSnapshot()
                {
                    LastModified = hasLastModified ? lastModified : "<original post date not recorded>",
                    Url = blobItem.SnapshotQualifiedUri.AbsoluteUri,
                    Comment = blobItem.SnapshotTime.Value.ToString()
                });
#endif
            }

            return faqSnapshots;
        }

        void FetchMeta(CloudBlockBlob blob)
        {
            try
            {
                blob.FetchAttributes();
            }
            catch (Exception ex)
            {
                var foo = ex;
            }
        }
    }
}
