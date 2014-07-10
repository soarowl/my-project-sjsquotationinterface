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
        public byte[] HeaderData { get; private set; }
        public byte[] BodyData { get; set; }

        private byte[] trailerData;
        public byte[] TrailerData
        {
            get { return trailerData; }
            set
            {
                this.trailerData = value;
                if (value != null)
                    this.Trailer = BigEndianStructHelper<Trailer>.BytesToStruct(this.TrailerData, MsgConsts.MsgEncoding);
                else
                    this.Trailer = default(Trailer);
            }
        }

        public Trailer Trailer { get; private set; }
        public bool Validate(out string msg)
        {

            if (this.Header.BodyLength > 0)
            {
                if (this.BodyData == null)
                {
                    msg = "消息体为null";
                    return false;
                }
                if (this.BodyData.LongLength != this.Header.BodyLength)
                {
                    msg = string.Format("消息体长度与预期不一致，Header.BodyLength={0},BodyData.LongLength ={1}", this.Header.BodyLength, this.BodyData.LongLength);
                    return false;
                }
                if (this.TrailerData == null || this.TrailerData.Length != tailSize)
                {
                    msg = "消息尾为null或不完整";
                    return false;
                }
                byte[] headerAndBody = DataHelper.UnionByteArrays(this.HeaderData, this.BodyData);
                var checksum = Trailer.GenerateChecksum(headerAndBody);

                if (Trailer.Checksum != checksum)
                {
                    msg = string.Format("校验和错误，Trailer.Checksum={0}，RealChecksum={1}", Trailer.Checksum, checksum);
                    return false;
                }
                msg = null;
                return true;
            }
            else
            {
                msg = null;
                return true;
            }
        }

        public string ToLogString()
        {
            var unioned = DataHelper.UnionByteArrays(this.HeaderData, this.BodyData, this.TrailerData);
            return string.Join(",", unioned);
        }
    }
}
