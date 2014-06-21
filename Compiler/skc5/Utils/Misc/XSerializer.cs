using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Collections;
using System.Reflection;

namespace SharpKit.Utils
{
    class XSerializer
    {

        bool IsPrimitive(Type type)
        {
            return (type.IsValueType || type == typeof(string));
        }

        public void SerializeToFile(object o, string filename)
        {
            XDocument doc = new XDocument();
            doc.Add(new XElement(o.GetType().Name));
            Serialize(o, doc.Root);
            doc.Save(filename);
        }

        public T DeserializeFromFile<T>(string filename) where T : new()
        {
            var doc = XDocument.Load(filename);
            T ret = new T();
            Deserialize<T>(doc.Root, ret);
            return ret;
        }

        public T DeserializeFromXml<T>(string xml) where T : new()
        {
            var doc = XDocument.Parse(xml);
            T ret = new T();
            Deserialize<T>(doc.Root, ret);
            return ret;
        }


        public void Serialize(object obj, XContainer parent)
        {
            if (obj == null)
                return;
            var type = obj.GetType();
            if (IsPrimitive(type))
            {
                parent.Add(new XText(XmlHelper.XmlConvertFrom(obj)));
            }
            else if (obj is IList)
            {
                var el = parent;// new XElement(XName.Get(type.Name));
                var list = (IList)obj;
                foreach (var item in list)
                {
                    var itemType = item.GetType();
                    var itemElement = new XElement(XName.Get(itemType.Name));
                    Serialize(item, itemElement);
                    el.Add(itemElement);
                }
            }
            else
            {
                var el = parent;// new XElement(XName.Get(type.Name));
                foreach (var pe in type.GetProperties())
                {
                    if (CanSerializeFunc != null && !CanSerializeFunc(obj, pe))
                        continue;

                    var value = pe.GetValue(obj, null);
                    if (value == null)
                        continue;

                    if (IsPrimitive(value.GetType()))
                    {
                        var peAtt = new XAttribute(XName.Get(pe.Name), value);
                        el.Add(peAtt);
                    }
                    else
                    {
                        var peElement = new XElement(XName.Get(pe.Name));
                        Serialize(value, peElement);
                        el.Add(peElement);
                    }
                }
            }
        }

        public Func<object, PropertyInfo, bool> CanSerializeFunc { get; set; }

        object Deserialize(Type type, string xmlValue)
        {
            var value = XmlHelper.XmlConvertTo(xmlValue, type);
            return value;
        }

        public T Deserialize<T>(XNode node, T obj)
        {
            return (T)Deserialize(typeof(T), node, obj);
        }

        public object Deserialize(Type type, XNode node, object obj)
        {
            if (IsPrimitive(type))
            {
                string text;
                if (node is XElement)
                    text = ((XElement)node).Value;
                else if (node is XText)
                    text = ((XText)node).Value;
                else
                    throw new Exception();
                var value = Deserialize(type, text);
                return value;
            }
            else if (node is XText)
            {
                var tn = (XText)node;
                var value = Deserialize(type, tn.Value);
                return value;
            }
            else if (node is XElement)
            {
                var el = (XElement)node;
                if (obj == null)
                {
                    obj = Activator.CreateInstance(type);
                }
                if (obj is IList)
                {
                    var list = (IList)obj;
                    foreach (var itemElement in el.Elements())
                    {
                        var itemType = ResolveType(EnumerableHelper.GetEnumerableItemType(type), itemElement.Name);
                        var item = Deserialize(itemType, itemElement, null);
                        list.Add(item);
                    }
                }
                else
                {
                    foreach (var peAtt in el.Attributes())
                    {
                        var peName = peAtt.Name.LocalName;
                        var pe = type.GetProperty(peName);
                        if (pe != null)
                        {
                            var value = Deserialize(pe.PropertyType, peAtt.Value);
                            if (value != null)
                                pe.SetValue(obj, value, null);
                        }
                        else
                        {
                            Warn("Property {0} not found.", peName);
                        }
                    }
                    foreach (var peElement in el.Elements())
                    {
                        var peName = peElement.Name.LocalName;
                        var pe = type.GetProperty(peName);
                        if (pe != null)
                        {
                            var currentValue = pe.GetValue(obj, null);
                            var value = Deserialize(pe.PropertyType, peElement, currentValue);
                            pe.SetValue(obj, value, null);
                        }
                        else
                        {
                            Warn("Property {0} not found.", peName);
                        }
                    }
                }
                return obj;
            }
            else
            {
                throw new Exception("Unknown node type: " + node);
            }
        }

        private void Warn(string format, params object[] args)
        {
        }

        private Type ResolveType(Type scope, XName xName)
        {
            var name = xName.LocalName;
            if (scope.Name == name)
                return scope;
            var type = scope.Assembly.GetType(scope.Namespace + "." + name);
            return type ?? scope;
        }

    }
}
