using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace QuotV5.Binary
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
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
        [MarshalAs(UnmanagedType.VariantBool)]//这里是为了在序列化时使其占用2个字节，而不是默认的4个
        public bool EndOfChannel;


        public MsgType MsgType
        {
            get { return MsgType.ChannelHeartbeat; }
        }

        public static ChannelHeartbeat Deserialize(byte[] bytes)
        {
            return BigEndianStructHelper<ChannelHeartbeat>.BytesToStruct(bytes, MsgConsts.MsgEncoding);
        }
    }
}
