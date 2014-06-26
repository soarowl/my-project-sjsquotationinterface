using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace QuotV5.Binary
{
    public struct Resend
    {

        public char ResendType;

        public UInt16 ChannelNo;

        public Int64 AppBegSeqNum;

        public Int64 AppEndSeqNum;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string NewsID;

        public char ResendStatus;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string RejectText;
    }
}
