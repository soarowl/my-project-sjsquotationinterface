using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace QuotV5.Binary
{
    public struct RealtimeStatus
    {
        public StandardHeader Header;

        public Int64 OrigTime;

        public UInt16 ChannelNo;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string SecurityID;
         
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string SecurityIDSource;
       
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string SecurityPreName;

        public UInt32 NoSwitch;
        
    }
}
