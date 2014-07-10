using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace QuotV5.Binary
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Logout : IMessage
    {
        /// <summary>
        /// 
        /// </summary>
        public Int32 SessionStatus;
        
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
        public string Text;

        public MsgType MsgType
        {
            get { return MsgType.Logout; }
        }
    }
}
