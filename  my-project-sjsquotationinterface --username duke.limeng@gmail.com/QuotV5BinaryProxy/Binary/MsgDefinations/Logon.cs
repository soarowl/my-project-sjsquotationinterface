using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ServiceManager.Utils;
namespace QuotV5.Binary
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Logon : IMessage
    {

        /// <summary>
        /// 发送方代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string SenderCompID;

        /// <summary>
        /// 接收方代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string TargetCompID;

        /// <summary>
        /// 心跳间隔，单位秒
        /// </summary>
        public int HeartBtInt;

        /// <summary>
        /// 密码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string Password;

        /// <summary>
        /// 二进制协议版本
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string DefaultApplVerID;

        /// <summary>
        /// 消息类型
        /// </summary>
        public MsgType MsgType { get { return Binary.MsgType.Logon; } }
    }
}
