using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuotV5.Binary
{
    public struct ChannelHeartbeat : IMessage
    {

        /// <summary>
        /// 频道代码
        /// </summary>
        public UInt16 ChannelNo;

        /// <summary>
        /// 最后一条行情消息的记录号
        /// </summary>
        public Int64 ApplLastSeqNum;

        /// <summary>
        /// 频道结束标志
        /// </summary>
        public bool EndOfChannel;


        public MsgType MsgType
        {
            get { return MsgType.Heartbeat; }
        }
    }
}
