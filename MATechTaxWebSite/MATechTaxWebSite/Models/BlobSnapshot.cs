﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MATechTaxWebSite.Models
{
    public class BlobSnapshot
    {
        public string Thumbprint { get; set; }
        public string Url { get; set; }
        public string LastModified { get; set; } // may be null
        public string WhenCaptured { get; set; }
        public string Comment { get; set; }
    }
}