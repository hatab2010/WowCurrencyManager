using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowCurrencyManager.v2
{
    static class Global
    {
        public const float ICOME_PROCENTAGE = 0.8f;

        public static Dictionary<string, string> Icons = new Dictionary<string, string>()
        {
            { "People", "https://www.flaticon.com/svg/vstatic/svg/681/681392.svg?token=exp=1614963024" +
                        "~hmac=0b7d3ad94ae0600223a2a50cb1f256c6"
            },
            { "Safe", "https://www.flaticon.com/premium-icon/icons/svg/3073/3073524.svg" }
        };
    }
}
