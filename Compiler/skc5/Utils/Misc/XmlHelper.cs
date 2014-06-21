using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace SharpKit.Utils
{
    static class XmlHelper
    {
        static XmlHelper()
        {
            PrimitiveTypes = new HashSet<Type> { typeof(int), typeof(bool), typeof(DateTime), typeof(string) };//TODO: fill this
        }
        public static string SerializeToXml(object obj)
        {
            if (obj == null)
                return null;
            var serializer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            serializer.Serialize(sw, obj);
            sw.Close();
            return sb.ToString();
        }
        public static string GetAttributeValue(XmlNode node, string attName)
        {
            XmlAttribute att = node.Attributes[attName];
            if (att != null)
                return att.Value;
            return null;
        }
        static HashSet<Type> PrimitiveTypes;

        public static bool IsPrimitive(Type type)
        {
            var uType = Nullable.GetUnderlyingType(type);
            if (uType != null)
                return IsPrimitive(uType);
            return PrimitiveTypes.Contains(type);
        }

        public static object XmlConvertTo(string xmlValue, Type type)
        {
            var nut = Nullable.GetUnderlyingType(type);
            if (nut != null)
            {
                if (xmlValue.IsNullOrEmpty())
                    return null;
                type = nut;
            }
            if (type == typeof(string))
            {
                return xmlValue;
            }
            else if (type == typeof(bool))
            {
                if (xmlValue == "True" || xmlValue == "true")
                    return true;
                if (xmlValue == "False" || xmlValue == "false")
                    return false;
                return XmlConvert.ToBoolean(xmlValue);
            }
            else if (type == typeof(int))
            {
                return XmlConvert.ToInt32(xmlValue);
            }
            else if (type == typeof(uint))
            {
                return XmlConvert.ToUInt32(xmlValue);
            }
            else if (type == typeof(long))
            {
                return XmlConvert.ToInt64(xmlValue);
            }
            else if (type == typeof(ulong))
            {
                return XmlConvert.ToUInt64(xmlValue);
            }
            else if (type == typeof(DateTime))
            {
                DateTime dt;
                if (DateTime.TryParse(xmlValue, out dt))
                    return dt;
                return XmlConvert.ToDateTime(xmlValue, XmlDateTimeSerializationMode.Unspecified);
            }
            else//TODO: complete this
                throw new NotSupportedException("XmlConvertTo is not supported for type " + type);
        }

        //TODO:
        public static string XmlConvertFrom(object value)
        {
            if (value == null)
                return null;
            return value.ToString();
            //var nut = Nullable.GetUnderlyingType(type);
            //if (nut != null)
            //{
            //  if (xmlValue.IsNullOrEmpty())
            //    return null;
            //  type = nut;
            //}
            //if (type == typeof(string))
            //{
            //  return xmlValue;
            //}
            //else if (type == typeof(bool))
            //{
            //  if (xmlValue == "True" || xmlValue == "true")
            //    return true;
            //  if (xmlValue == "False" || xmlValue == "false")
            //    return false;
            //  return XmlConvert.ToBoolean(xmlValue);
            //}
            //else if (type == typeof(int))
            //{
            //  return XmlConvert.ToInt32(xmlValue);
            //}
            //else if (type == typeof(uint))
            //{
            //  return XmlConvert.ToUInt32(xmlValue);
            //}
            //else if (type == typeof(long))
            //{
            //  return XmlConvert.ToInt64(xmlValue);
            //}
            //else if (type == typeof(ulong))
            //{
            //  return XmlConvert.ToUInt64(xmlValue);
            //}
            //else if (type == typeof(DateTime))
            //{
            //  DateTime dt;
            //  if (DateTime.TryParse(xmlValue, out dt))
            //    return dt;
            //  return XmlConvert.ToDateTime(xmlValue, XmlDateTimeSerializationMode.Unspecified);
            //}
            //else//TODO: complete this
            //  throw new NotSupportedException("XmlConvertTo is not supported for type " + type);
        }


    }
}
