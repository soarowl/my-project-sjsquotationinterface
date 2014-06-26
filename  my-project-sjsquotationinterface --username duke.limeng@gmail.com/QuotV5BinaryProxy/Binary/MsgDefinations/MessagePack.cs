using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceManager.Utils;
using System.Runtime.InteropServices;
namespace QuotV5.Binary
{
    public class MessagePack
    {
        private static int tailSize = Marshal.SizeOf(typeof(Trailer));
        public MessagePack(StandardHeader header)
        {
            this.Header = header;
        }
        public MessagePack(byte[] headerBytes)
        {
            this.HeaderData = headerBytes;
            Header = BigEndianStructHelper<StandardHeader>.BytesToStruct(headerBytes, MsgConsts.MsgEncoding);

        }

        public StandardHeader Header { get; private set; }
        public byte[] HeaderData { get;private set; }
        public byte[] BodyData { get; set; }
        public byte[] TailData { get; set; }

        public bool Validate()
        {

            if (this.Header.BodyLength > 0)
            {
                if (this.BodyData == null)
                    return false;
                if (this.BodyData.LongLength != this.Header.BodyLength)
                    return false;
                if (this.TailData == null || this.TailData.Length != tailSize)
                    return false;
                byte[] headerAndBody = DataHelper.UnionByteArrays(this.HeaderData, this.BodyData);
                var checksum = Trailer.GenerateChecksum(headerAndBody);

                Trailer tail = BigEndianStructHelper<Trailer>.BytesToStruct(this.TailData, MsgConsts.MsgEncoding);
                if (tail.Checksum != checksum)
                    return false;

                return true;
            }
            else
            {
                return true;
            }
        }
    }
}
