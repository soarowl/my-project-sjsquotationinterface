using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;

namespace QuotV5.StaticInfo
{
    public abstract class ParserBase
    {
        protected T ParseNode<T>(XElement node)
            where T : new()
        {
            var properties = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            T rtn = ObjectCreator<T>.Instance.Create(Type.EmptyTypes, null);

            foreach (var property in properties)
            {

                if (ParseSpecialPropery<T>(rtn, property, node))
                {
                    continue;
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


        protected abstract bool ParseSpecialPropery<T>(T obj, PropertyInfo property, XElement node);


        protected static string ReadValueStr(XElement securityInfoNode, string propertyName)
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
}
