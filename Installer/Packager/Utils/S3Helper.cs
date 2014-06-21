using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon;
using System.Net;
using Amazon.S3.Transfer;
using Amazon.S3.Model;

namespace SharpKit.Utils
{
    class S3Helper
    {
        public static void Upload(string filename, string url, EventHandler<UploadProgressArgs> progressCallback)
        {
            var uri = new Uri(url);
            var bucketName = uri.Host;
            var key = uri.PathAndQuery.Remove(0, 1);
            var s3 = AWSClientFactory.CreateAmazonS3Client("AKIAIJSGSOQLHZIKI4PQ", @"W6OCRQbMDpUP8e2OYnc++GHq2fTchGIB8cm0Om9D");
            var util = new TransferUtility(s3);
            var req = new TransferUtilityUploadRequest { BucketName = bucketName, Key = key, FilePath = filename, CannedACL = S3CannedACL.PublicRead, Timeout = (int)TimeSpan.FromHours(24).TotalMilliseconds };
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            req.UploadProgressEvent += progressCallback;
            util.Upload(req);
        }
    }
}
