using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExternalCredit.CrawlerLayer.Assure.Handlers
{
    public class TimeLimitHandler : BaseCreditQueryHandler
    {
        protected override HandleResponse Handle(HandleRequest request)
        {
            var res = new HandleResponse();
            var now = DateTime.Now;
            if (now < Convert.ToDateTime(ConfigData.TimeLimit[0])
                || now > Convert.ToDateTime(ConfigData.TimeLimit[1]))
            {
                Console.WriteLine("时间限制外无法操作");
                Log4netAdapter.WriteInfo("时间限制外无法操作");

                res.DoNextHandler = false;
                res.Description = "时间限制外无法操作";
            }
            else
                res.DoNextHandler = true;

            return res;
        }

        protected override void SetNextRequest(HandleRequest request, HandleResponse res)
        {
            return;
        }
    }
}
