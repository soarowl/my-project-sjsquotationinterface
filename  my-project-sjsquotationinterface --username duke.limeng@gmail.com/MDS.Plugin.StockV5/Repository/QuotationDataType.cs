using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDS.Plugin.SZQuotV5
{
    /// <summary>
    /// 行情信息类型
    /// </summary>
    public enum QuotationDataType : int
    {
        /// <summary>
        /// 证券信息
        /// </summary>
        StockInfo = 2,

        /// <summary>
        /// 动态行情
        /// </summary>
        Quotation = 3,

    }

    /// <summary>
    /// 格式化方法
    /// </summary>
    static partial class Format
    {
        /// <summary>
        /// QuotationDataType的格式化方法
        /// </summary>
        /// <param name="qdt"></param>
        /// <returns></returns>
        public static string ToFormatString(this QuotationDataType qdt)
        {
            return ((int)qdt).ToString("00");
        }

        /// <summary>
        /// QuotationSource的格式化方法
        /// </summary>
        /// <param name="qs"></param>
        /// <returns></returns>
        public static string ToFormatString(this QuotationSource qs)
        {
            return qs.ToString().PadLeft(3, '0');
        }
    }
}
