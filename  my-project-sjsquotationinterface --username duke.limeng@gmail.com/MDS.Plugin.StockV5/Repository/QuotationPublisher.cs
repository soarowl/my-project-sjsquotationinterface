using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ServiceManager.Services;
using System.Collections;

namespace MDS.Plugin.SZQuotV5
{
    /// <summary>
    /// 行情订阅发布者
    /// </summary>
    [DebuggerNonUserCode]
    public  class QuotationPublisher
    {

        /// <summary>
        /// 允许与每个行情查询服务器建立多少个连接
        /// 如果查询服务设置为使用发布服务器，则会共享这些连接数
        /// </summary>
        internal static Int32 ConnNumPerService = 10;
        /// <summary>
        /// 发布者
        /// </summary>
        internal  static readonly  QuotationPublishService Publisher= DirectoryService.GetQuotationPublishService(QuotServiceNames.PubServicePrefix +StockQuotationSource.Level1, ConnNumPerService);
                 
    }
}
