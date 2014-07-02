using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace QuotV5.Binary
{
    public class RealtimeStatus : IMarketData
    {
        private static int switchSize = Marshal.SizeOf(typeof(SecuritySwitch));
        private static int statusSize = Marshal.SizeOf(typeof(RealtimeStatusWithoutSwitches));

        public RealtimeStatusWithoutSwitches Status { get; set; }

        public SecuritySwitch[] Switches { get; set; }

        public static  RealtimeStatus Deserialize(byte[] bytes)
        {
            if (bytes == null || bytes.Length < statusSize)
                return null;

            byte[] b1 = bytes.Take(statusSize).ToArray();
            RealtimeStatusWithoutSwitches status = BigEndianStructHelper<RealtimeStatusWithoutSwitches>.BytesToStruct(b1, MsgConsts.MsgEncoding);

            if (status.NoSwitch > 0 && status.NoSwitch * switchSize + switchSize <= bytes.Length)
            {
                SecuritySwitch[] switches = new SecuritySwitch[status.NoSwitch];


                for (int i = 0; i < status.NoSwitch; i++)
                {
                    int offset = statusSize + switchSize * i;
                    byte[] b2 = bytes.Skip(offset).Take(switchSize).ToArray();
                    SecuritySwitch s = BigEndianStructHelper<SecuritySwitch>.BytesToStruct(b2, MsgConsts.MsgEncoding);
                    switches[i] = s;
                }
                RealtimeStatus rtn = new RealtimeStatus()
                {
                    Status = status,
                    Switches = switches
                };
                return rtn;
            }
            else
            {
                return null;
            }


        }

    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RealtimeStatusWithoutSwitches
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
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SecuritySwitch
    {
        public SecuritySwitchType Type;
        [MarshalAs(UnmanagedType.VariantBool)]//这里是为了在序列化时使其占用2个字节，而不是默认的4个
        public bool Status;
    }
}
