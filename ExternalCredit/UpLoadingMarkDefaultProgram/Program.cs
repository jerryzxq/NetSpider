using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Vcredit.Common.Ext;
using Vcredit.Common.Utility;
using Vcredit.ExtTrade.BusinessLayer;

namespace UpLoadingMarkDefaultProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            Log4netAdapter.WriteInfo(string.Format("开始标记===================="));

            int? diffHour = ConfigurationManager.AppSettings["diffHour"].ToInt();
            if (diffHour == null || diffHour >= 1)
                throw new ArgumentException("diffHour");

            new CRD_CD_AssureReportedInfoBusiness().UpLoadingMarkDefault(diffHour.Value);

            new CRD_CD_AssureMaintainBusiness().UpLoadingMarkDefault(diffHour.Value);

            new CRD_CD_AssureMaintainBalanceTransferBusiness().UpLoadingMarkDefault(diffHour.Value);

            new CRD_CD_AssureReportedAfterInfoBusiness().UpLoadingMarkDefault(diffHour.Value);

            Log4netAdapter.WriteInfo(string.Format("结束标记===================="));

            Thread.Sleep(5000);
            
        }
    }
}
