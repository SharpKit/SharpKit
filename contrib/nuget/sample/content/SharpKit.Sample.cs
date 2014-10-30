using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SharpKit.JavaScript;
using SharpKit.Html;

//Install the package SharpKit.jQuery for jQuery C# support! (SharpKit.jQuery.dll will be added)
//using SharpKit.jQuery;

namespace SharpKitSample
{

    [JsType(JsMode.Prototype, Filename = "SharpKit.Sample.js")]
    public class SampleClass
    {

        public static void RunSample()
        {
            HtmlDocument doc = HtmlContext.document;
            doc.write("Hello World!");
            HtmlContext.alert("Welcome!");
        }

    }

}