using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDS.Plugin.SZQuotV5
{
    public class JsonSerializer
    {
        public static byte[] ObjectToBytes<T>(T obj)
        {
            var str = ServiceStack.Text.JsonSerializer.SerializeToString<T>(obj);
            return Encoding.UTF8.GetBytes(str);
        }
        public static string ObjectToString<T>(T obj)
        {
            return ServiceStack.Text.JsonSerializer.SerializeToString<T>(obj);
        }
        public static T BytesToObject<T>(byte[] bytes)
        {
            string str = Encoding.UTF8.GetString(bytes);
            return ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(str);
        }
        public static T StringToObject<T>(string str)
        {
            return ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(str);
        }
    }
}
