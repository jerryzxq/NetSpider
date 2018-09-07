using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Ext;
using Vcredit.Common.Utility;
using Vcredit.ExtTrade.BusinessLayer;

namespace AssureMarkFailProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            Log4netAdapter.WriteInfo(string.Format("开始标记===================="));

            int? diffday = ConfigurationManager.AppSettings["diffDay"].ToInt();
            if (diffday == null || diffday >= 1)
                throw new ArgumentException("请配置正确的diffday参数 ");
            
            new CRD_CD_CreditUserInfoBusiness().ConnectionFailedMarkFail(diffday.Value);

            Log4netAdapter.WriteInfo(string.Format("结束标记===================="));
        }
    }
}
