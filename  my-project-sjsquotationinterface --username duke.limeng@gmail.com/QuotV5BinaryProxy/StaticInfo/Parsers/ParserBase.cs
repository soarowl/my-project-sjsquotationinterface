using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using System.Collections;

namespace QuotV5.StaticInfo
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// 性能有损失，但是调用不频繁的话，就无所谓了
    /// </remarks>
    public abstract class ParserBase
    {

        protected T ParseNode<T>(XContainer node)
            where T : new()
        {
            var properties = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            T rtn = ObjectCreator<T>.Instance.Create(Type.EmptyTypes, null);

            foreach (var property in properties)
            {

                if (property.PropertyType.IsGenericType)
                {
                    XmlParseInfoAttribute parseInfo = XmlParseInfoAttribute.GetAttribute(property);
                    var value = ParseList(property.PropertyType, node, parseInfo.NodeName, parseInfo.ChildNodeName);
                    FastReflection.SetPropertyValue<object>(rtn, property, value);
                }
                else
                {

                    string valueStr = ReadValueStr(node, property.Name);
                    if (!string.IsNullOrEmpty(valueStr))
                    {
                        if (property.PropertyType == typeof(string))
                        {
                            FastReflection<T>.SetPropertyValue<string>(rtn, property, valueStr);
                        }
                        else if (property.PropertyType == typeof(Int32))
                        {
                            int value = DataHelper.TryParseInt32(valueStr);
                            FastReflection<T>.SetPropertyValue<Int32>(rtn, property, value);
                        }
                        else if (property.PropertyType == typeof(Int64))
                        {
                            Int64 value = DataHelper.TryParseInt64(valueStr);
                            FastReflection<T>.SetPropertyValue<Int64>(rtn, property, value);
                        }
                        else if (property.PropertyType == typeof(Double))
                        {
                            Double value = DataHelper.TryParseDouble(valueStr);
                            FastReflection<T>.SetPropertyValue<Double>(rtn, property, value);
                        }
                        else if (property.PropertyType == typeof(Decimal))
                        {
                            Decimal value = DataHelper.TryParseDecimal(valueStr);
                            FastReflection<T>.SetPropertyValue<Decimal>(rtn, property, value);
                        }
                        else if (property.PropertyType == typeof(Boolean))
                        {
                            Boolean value = DataHelper.TryParseBoolean(valueStr);
                            FastReflection<T>.SetPropertyValue<Boolean>(rtn, property, value);
                        }
                        else if (property.PropertyType.IsEnum)
                        {
                            int value = DataHelper.TryParseInt32(valueStr);

                            if (Enum.IsDefined(property.PropertyType, value))
                            {
                                FastReflection<T>.SetPropertyValue<object>(rtn, property, value);
                            }
                        }
                    }
                }

            }
            return rtn;
        }


        protected List<T> ParseCollection<T>(XContainer node, string rootNodeName, string childNodeName)
            where T : new()
        {
            if (!node.Descendants(rootNodeName).Any())
                return null;
            var childNodes = node.Descendants(rootNodeName).First().Descendants(childNodeName);
            var count = childNodes.Count();
            if (count > 0)
            {
                List<T> rtn = new List<T>(count);
                foreach (var childNode in childNodes)
                {
                    T obj = ParseNode<T>(childNode);
                    rtn.Add(obj);
                }
                return rtn;
            }
            else
            {
                return null;
            }
        }


        protected object ParseNode(Type returnType, XContainer node)
        {
            var properties = returnType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            object rtn = ObjectCreator.Create(returnType, Type.EmptyTypes, null);

            foreach (var property in properties)
            {
                string valueStr = ReadValueStr(node, property.Name);
                if (!string.IsNullOrEmpty(valueStr))
                {
                    if (property.PropertyType == typeof(string))
                    {
                        FastReflection.SetPropertyValue<string>(rtn, property, valueStr);
                    }
                    else if (property.PropertyType == typeof(Int32))
                    {
                        int value = DataHelper.TryParseInt32(valueStr);
                        FastReflection.SetPropertyValue<Int32>(rtn, property, value);
                    }
                    else if (property.PropertyType == typeof(Int64))
                    {
                        Int64 value = DataHelper.TryParseInt64(valueStr);
                        FastReflection.SetPropertyValue<Int64>(rtn, property, value);
                    }
                    else if (property.PropertyType == typeof(Double))
                    {
                        Double value = DataHelper.TryParseDouble(valueStr);
                        FastReflection.SetPropertyValue<Double>(rtn, property, value);
                    }
                    else if (property.PropertyType == typeof(Decimal))
                    {
                        Decimal value = DataHelper.TryParseDecimal(valueStr);
                        FastReflection.SetPropertyValue<Decimal>(rtn, property, value);
                    }
                    else if (property.PropertyType == typeof(Boolean))
                    {
                        Boolean value = DataHelper.TryParseBoolean(valueStr);
                        FastReflection.SetPropertyValue<Boolean>(rtn, property, value);
                    }
                    else if (property.PropertyType.IsEnum)
                    {
                        int value = DataHelper.TryParseInt32(valueStr);

                        if (Enum.IsDefined(property.PropertyType, value))
                        {
                            FastReflection.SetPropertyValue<object>(rtn, property, value);
                        }
                    }
                }
                else if (property.PropertyType.IsGenericType)
                {
                    XmlParseInfoAttribute parseInfo = XmlParseInfoAttribute.GetAttribute(property);
                    var value = ParseList(property.PropertyType, node, parseInfo.NodeName, parseInfo.ChildNodeName);
                    FastReflection.SetPropertyValue<object>(rtn, property, value);
                }
            }
            return rtn;
        }

        protected object ParseList(Type listType, XContainer node, string rootNodeName, string childNodeName)
        {
            if (!listType.IsGenericType)
                return null;

            if (!node.Descendants(rootNodeName).Any())
                return null;

            Type objType = listType.GetGenericArguments().First();

            var childNodes = node.Descendants(rootNodeName).First().Descendants(childNodeName);
            var count = childNodes.Count();
            if (count > 0)
            {
                var rtn = ObjectCreator.Create(listType, Type.EmptyTypes, null);
                foreach (var childNode in childNodes)
                {
                    object obj = ParseNode(objType, childNode);
                    FastReflection.AddObjectToList(rtn, obj);
                }
                return rtn;
            }
            else
            {
                return null;
            }
        }
        
        protected static string ReadValueStr(XContainer securityInfoNode, string propertyName)
        {
            if (securityInfoNode.Element(propertyName) != null)
            {
                var rtn = securityInfoNode.Element(propertyName).Value;
                if (!string.IsNullOrEmpty(rtn))
                    return rtn.Trim();
                else
                    return null;
            }
            else
            {
                return null;
                // throw new System.Xml.XmlException(string.Format("未找到节点{0}", propertyName));
            }
        }

    }

    public abstract class ParserBase<TReturn, TParser> : ParserBase
        where TReturn : new()
        where TParser : ParserBase<TReturn, TParser>
    {
        public List<TReturn> Parse(string xmlContent)
        {
            XDocument xDoc = XDocument.Parse(xmlContent, LoadOptions.None);
            var parseInfo = XmlParseInfoAttribute.GetAttribute(typeof(TReturn));
            if (parseInfo != null)
            {
                return ParseCollection<TReturn>(xDoc, parseInfo.NodeName, parseInfo.ChildNodeName);
            }
            else
            {
                throw new Exception(string.Format("需要为类型{0}附加XmlParseInfoAttribute才能够进行解析", typeof(TReturn).Name));

            }
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
    public class XmlParseInfoAttribute : Attribute
    {
        public string NodeName { get; private set; }
        public string ChildNodeName { get; private set; }

        public XmlParseInfoAttribute(string nodeName, string childNodeName)
        {
            this.NodeName = nodeName;
            this.ChildNodeName = childNodeName;
        }

        public static XmlParseInfoAttribute GetAttribute(Type type)
        {
            return Attribute.GetCustomAttribute(type, typeof(XmlParseInfoAttribute)) as XmlParseInfoAttribute;
        }
        public static XmlParseInfoAttribute GetAttribute(PropertyInfo propertyInfo)
        {
            return Attribute.GetCustomAttribute(propertyInfo, typeof(XmlParseInfoAttribute), false) as XmlParseInfoAttribute;
        }


    }
}
