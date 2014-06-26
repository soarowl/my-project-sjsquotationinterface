using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ServiceManager.Utils;
namespace QuotV5.Binary
{
    public class MessageHelper
    {
        public static byte[] ComposeMessage<TMsg>(TMsg message)
            where TMsg : struct,IMessage
        {

            int dataLength = Marshal.SizeOf(typeof(TMsg));
            StandardHeader header = new StandardHeader()
            {
                Type =(UInt32) message.MsgType,
                BodyLength = (uint)dataLength
            };
         
            byte[] headerBytes = BigEndianStructHelper<StandardHeader>.StructToBytes(header, MsgConsts.MsgEncoding);
            byte[] headerAndBody = null;

            if (dataLength > 0)
            {
                byte[] bodyBytes = BigEndianStructHelper<TMsg>.StructToBytes(message, MsgConsts.MsgEncoding);
                headerAndBody = DataHelper.UnionByteArrays(headerBytes, bodyBytes);
            }
            else
                headerAndBody = headerBytes;

            Trailer trailer = new Trailer()
            {
                Checksum = Trailer.GenerateChecksum(headerAndBody)
            };

            byte[] trailerBytes = BigEndianStructHelper<Trailer>.StructToBytes(trailer, MsgConsts.MsgEncoding);

            byte[] rtn =DataHelper.UnionByteArrays(headerAndBody, trailerBytes);

            return rtn;
        }
    }
}
