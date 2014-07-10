using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDS.Plugin.SZQuotV5
{
    /// <summary>
    /// MQ消息类型
    /// </summary>
    public enum MQMsgType
    {
        /// <summary>
        /// 未设置消息类型
        /// </summary>
        UNSET = 0,
        /// <summary>
        /// 主题类型
        /// </summary>
        TOPIC = 1,
        /// <summary>
        /// 队列类型
        /// </summary>
        QUEUE = 2
    }
}
