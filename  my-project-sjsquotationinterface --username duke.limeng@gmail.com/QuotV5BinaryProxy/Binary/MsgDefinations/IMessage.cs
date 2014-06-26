using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuotV5.Binary
{
    public interface IMessage
    {
        MsgType MsgType { get; }
    }
}
