using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuotV5
{
    class DataHelper
    {

        public static byte[] UnionByteArrays(params byte[][] byteArrays)
        {
            byte[] rtn = new byte[byteArrays.Where(ba => ba != null).Sum(ba => ba.Length)];
            int offset = 0;
            foreach (var ba in byteArrays)
            {
                if (ba != null)
                {
                    ba.CopyTo(rtn, offset);
                    offset += ba.Length;
                }
            }
            return rtn;
        }



        /// <summary>
        /// 将字符串转换为int
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int TryParseInt32(string str)
        {
            int rtn;
            if (int.TryParse(str, out rtn))
            {
                return rtn;
            }
            else
            {
                double dRtn;
                if (double.TryParse(str, out dRtn))//某些时候，一个整数字段会被保存为double格式，如15.0
                {
                    return (int)dRtn;
                }
                else
                    return 0;
            }
        }

        /// <summary>
        /// 将字符串转换为Int64
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long TryParseInt64(string str)
        {
            long rtn;
            long.TryParse(str, out rtn);
            return rtn;
        }

        /// <summary>
        /// 将字符串转为double
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static double TryParseDouble(string str)
        {
            double rtn;
            double.TryParse(str, out rtn);
            return rtn;
        }
        /// <summary>
        /// 将字符串转为double
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Decimal TryParseDecimal(string str)
        {
            Decimal rtn;
            Decimal.TryParse(str, out rtn);
            return rtn;
        }

        public static Boolean TryParseBoolean(string str)
        {
            Boolean rtn = false;
            if (!string.IsNullOrEmpty(str))
            {
                if (string.Equals(str, "Y", StringComparison.OrdinalIgnoreCase))
                {
                    rtn = true;
                }
                else if (string.Equals(str, "N", StringComparison.OrdinalIgnoreCase))
                {
                    rtn = false;
                }
            }
            return rtn;
        }

    }
}
