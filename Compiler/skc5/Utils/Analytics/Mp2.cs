using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SharpKit.Utils.Analytics
{
    class Mp
    {
        void Invoke()
        {
            //var url = "http://www.google-analytics.com/collect";
            //var req = (HttpWebRequest)HttpWebRequest.Create();
            //var client = new WebClient();
            //client.UploadDataAsync(url, );
        }
    }

    class MpRequest
    {
        /// <summary>
        /// Version (set to 1)
        /// </summary>
        public string v { get; set; }
        /// <summary>
        /// Tracking ID / Web property / Property ID. UA-XXXX-Y
        /// </summary>
        public string tid { get; set; }
        /// <summary>
        /// Anonymous Client ID. 555
        /// </summary>
        public string cid { get; set; }
        /// <summary>
        /// Hit Type
        /// </summary>
        public string t { get; set; }
    }
}
