using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuotV5.StaticInfo
{
   public enum SecurityStatus
    {
        停牌 = 1,
        除权 = 2,
        除息 = 3,
        ST = 4,
        StarST = 5,
        上市首日 = 6,
        未完成股改 = 7,
        恢复上市首日 = 8,
        股改复牌上市首日 = 9,
        退市整理期 = 10,
        暂停上市 = 11,
        增发股份上市=12

    }
}
