using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuotV5.StaticInfo
{
    public class EVoteParams
    {
        /// <summary>
        /// 证券代码
        /// </summary>
        public string SecurityID { get; set; }

        /// <summary>
        /// 证券简称
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// 正股代码
        /// </summary>
        public string UnderlyingSecurityID { get; set; }

        /// <summary>
        /// 议案数
        /// </summary>
        public Int32 ProposalNum { get; set; }

    }
}
