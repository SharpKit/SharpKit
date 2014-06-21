using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpKit.Utils.Http
{
    class JsonServer
    {
        public object Service { get; set; }
        void ProcessRequest(HttpListenerContext context)
        {
            var action = context.Request.Url.AbsolutePath.Substring(1);
            var me = Service.GetType().GetMethod(action);
            var prms = me.GetParameters();
            var prms2 = new List<object>();
            if (prms.Length > 0)
            {
                var prmType = prms[0].ParameterType;
                object prm;
                if (context.Request.HttpMethod == "POST")
                {
                    var ser = new DataContractJsonSerializer(prmType);
                    prm = ser.ReadObject(context.Request.InputStream);
                }
                else
                {
                    var q = context.Request.QueryString;
                    prm = DeserializeFromQueryString(prmType, q);
                }
                prms2.Add(prm);
            }
            try
            {
                var returnValue = me.Invoke(Service, prms2.ToArray());
                context.Response.StatusCode = 200;
                if (returnValue != null)
                {
                    var ser2 = new DataContractJsonSerializer(returnValue.GetType());
                    ser2.WriteObject(context.Response.OutputStream, returnValue);
                }
                else
                {
                    context.Response.ContentLength64 = 0;
                }
                context.Response.OutputStream.Flush();
                context.Response.Close();
            }
            catch (TargetInvocationException e)
            {
                context.Response.StatusCode = 500;
                context.Response.StatusDescription = e.Message;
                context.Response.Close();
            }
        }

        public event Action<DeserializeFromQueryStringEventArgs> DeserializeFromQueryStringOverride;
        
        public object DeserializeFromQueryString(Type type, NameValueCollection q)
        {
            if (DeserializeFromQueryStringOverride != null)
            {
                var e = new DeserializeFromQueryStringEventArgs { Type = type, QueryString = q };
                DeserializeFromQueryStringOverride(e);
                if (e.Handled)
                    return e.Result;
            }
            var prm = Activator.CreateInstance(type);
            foreach (string key in q)
            {
                var sValue = q[key];
                object value = sValue;
                var pe = prm.GetType().GetProperty(key, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                if (pe.PropertyType != typeof(string))
                {
                    value = Convert.ChangeType(sValue, pe.PropertyType);
                }
                pe.SetValue(prm, value);
            }
            return prm;
        }
        HttpListener Listener;

        Thread Thread;
        public void Start()
        {
            if (Listener != null)
                Stop();
            Thread = new Thread(Run);
            Thread.Start();
        }
        public void Stop()
        {
            if (Listener != null)
            {
                Listener.Abort();
                Listener = null;
                Thread.Join(1000);
            }
        }
        public string Url { get; set; }
        public void Run()
        {
            Listener = new HttpListener();
            Listener.Prefixes.Add(Url);
            Listener.Start();
            while (true)
            {
                try
                {
                    if (Listener == null || !Listener.IsListening)
                        break;
                    var context = Listener.GetContext();
                    Console.WriteLine("Started ", context.Request);
                    var ms = StopwatchHelper.TimeInMs(() => ProcessRequest(context));
                    Console.WriteLine("Finished {0}ms", ms);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }

    public class DeserializeFromQueryStringEventArgs : EventArgs
    {
        public Type Type { get; set; }
        public object Result { get; set; }
        public bool Handled { get; set; }

        public NameValueCollection QueryString { get; set; }
    }
}
