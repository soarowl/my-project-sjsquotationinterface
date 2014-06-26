using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace QuotV5.Binary
{
    public struct Logout
    {
        /// <summary>
        /// 
        /// </summary>
        public Int32 SessionStatus;
        
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 58)]
        public string Text;
    }
}
