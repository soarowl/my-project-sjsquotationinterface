using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace QuotV5.Binary
{
    public class BinaryQuotProxyConfig
    {
        public IPAddress IP { get; set; }
        public int Port_Realtime { get; set; }
        public int Port_Resend { get; set; }
        public string SenderCompID { get; set; }
        public string Password { get; set; }
        public int ConnectionTimeoutMS { get; set; }
        public int ReconnectIntervalMS { get; set; }
        public int HeartbeatIntervalS { get; set; }
    }
}
