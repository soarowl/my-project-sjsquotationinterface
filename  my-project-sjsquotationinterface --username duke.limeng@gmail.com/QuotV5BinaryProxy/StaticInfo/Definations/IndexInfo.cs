using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuotV5.StaticInfo
{
    /// <summary>
    /// 指数信息
    /// </summary>
    [XmlParseInfo("Securities","Security")]
    public class IndexInfo
    {
        /// <summary>
        /// 证券代码
        /// </summary>
        /// <remarks>
        /// C8
        /// </remarks>
        public string SecurityID { get; set; }

        /// <summary>
        /// 证券简称
        /// </summary>
        /// <remarks>
        /// C40
        /// </remarks>
        public string Symbol { get; set; }

        /// <summary>
        /// 英文简称
        /// </summary>
        /// <remarks>
        /// C40
        /// </remarks>       
        public string EnglishName { get; set; }
        
        /// <summary>
        /// 昨日收盘价
        /// </summary>
        /// <remarks>
        /// N18(5)
        /// </remarks>
        public decimal PrevCloseIdx { get; set; }

    }
}
