﻿using MATechTaxWebSite.Models;
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
            var blobLastModified = blob.Metadata["LastModified"];

            var blobOptions = Microsoft.WindowsAzure.Storage.Blob.BlobListingDetails.Snapshots; // | Microsoft.WindowsAzure.Storage.Blob.BlobListingDetails.Metadata;
            foreach (CloudBlockBlob blobItem in blob.Container.ListBlobs(prefix: null, useFlatBlobListing: true, blobListingDetails: blobOptions))
            {
                var lastModified = String.Empty;
                var thumbprint = String.Empty;
                var comment = String.Empty;
                var hasLastModified = blobItem.Metadata.TryGetValue("LastModified", out lastModified);
                var hasThumbprint = blobItem.Metadata.TryGetValue("thumbprint", out thumbprint);
                var hasComment = blobItem.Metadata.TryGetValue("comment", out comment);
                var lastModifiedText = hasLastModified ? lastModified : "{unknown}";
                if (!blobItem.IsSnapshot) lastModifiedText = blobLastModified;
                faqSnapshots.Add(new BlobSnapshot()
                {
                    Thumbprint = thumbprint,
                    LastModified = lastModifiedText,
                    WhenCaptured = blobItem.SnapshotTime.HasValue ? blobItem.SnapshotTime.ToString() + " UTC" : "N/A",
                    Url = blobItem.SnapshotQualifiedUri.AbsoluteUri,
                    Comment = blobItem.IsSnapshot ? comment : "(Most Current Version)"
                });
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
