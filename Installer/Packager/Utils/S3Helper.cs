using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Amazon;
using System.Net;
//using Amazon.S3.Transfer;
//using Amazon.S3.Model;
using System.Diagnostics;
using System.IO;

namespace SharpKit.Utils
{
    /*    class S3Helper
    {
        public static void Upload(string filename, string url, EventHandler<UploadProgressArgs> progressCallback)
        {
            var uri = new Uri(url);
            var bucketName = uri.Host;
            var key = uri.PathAndQuery.Remove(0, 1);
            var pairs = Process.GetCurrentProcess().MainModuleFilename().ToFileInfo().GetParent().Parent.GetFile("config.ini.user").Lines().Select(t => t.Split('=')).ToList();
            var ack = pairs.Where(t => t[0] == "AwsAccessKey").First()[1];
            var asak = pairs.Where(t => t[0] == "AwsSecretAccessKey").First()[1];
            var s3 = AWSClientFactory.CreateAmazonS3Client(ack, asak);
            ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };

            var util = new TransferUtility(s3);
            var req = new TransferUtilityUploadRequest { BucketName = bucketName, Key = key, FilePath = filename, CannedACL = S3CannedACL.PublicRead, Timeout = (int)TimeSpan.FromHours(24).TotalMilliseconds };
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            req.UploadProgressEvent += progressCallback;
            util.Upload(req);
        }
    }*/
}
