using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuotV5.Binary
{
    public enum SessionStatus
    {
        会话活跃=0,
        会话口令已更改=1,
        将过期的会话口令=2,
        新会话口令不符合规范=3,
        会话登录完成=4,
        不合法的用户名或口令=5,
        账户锁定=6,
        当前时间不允许登录=7,
        口令过期=8,
        收到的MsgSeqNum34太小=9,
        收到的NextExpectedMsgSeqNum789太大,
        其他=101,
        无效消息=102
    }
}
