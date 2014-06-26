using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace QuotV5.Binary
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Trailer
    {
        public uint Checksum;

        public static uint GenerateChecksum(char[] buffer)
        {
            uint sum = 0;
            foreach (var c in buffer)
                sum += (uint)c;
            return sum % 256;
        }

        public static uint GenerateChecksum(byte[] buffer)
        {
            uint sum = 0;
            foreach (var c in buffer)
                sum += (uint)c;
            return sum % 256;
        }
    }
}
