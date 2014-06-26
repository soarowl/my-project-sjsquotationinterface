using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace QuotV5.Binary
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StandardHeader
    {
        public UInt32 Type;
        public UInt32 BodyLength;
    }
}
