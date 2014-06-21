using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SharpKit.Release.Utils
{
    static class XExtensions
    {
        public static T GetChildValue<T>(this XElement el, string name)
        {
            var att = el.Attribute(name);
            string value = null;
            if (att == null)
            {
                var ch = el.Element(name);
                if (ch != null)
                    value = ch.Value;
            }
            else
            {
                value = att.Value;
            }
            if(value==null)
                return default(T);
            if (typeof(T) == typeof(string))
                return (T)(object)value;
            return (T)XmlHelper.XmlConvertTo(value, typeof(T));
        }
    }
}
