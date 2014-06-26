using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace QuotV5.Binary
{
    public struct Bullet
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string NewsID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Headline;

        public Int64 OrigTime;
        
        public UInt32 RawDataLength;
    }
}
