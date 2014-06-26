using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuotV5.Binary
{
    public struct Heartbeat : IMessage 
    {
        public MsgType MsgType
        {
            get {return MsgType.Heartbeat; }
        }
    }
}
