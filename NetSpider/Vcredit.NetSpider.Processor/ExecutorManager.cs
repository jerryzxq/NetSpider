using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Processor
{
   public class ExecutorManager
    {
        /// <summary>
        /// 调用接口IExecutor
        /// </summary>
        /// <returns></returns>
       public static IExecutor GetExecutor()
       {
           return new BaseExecutor();
       }
       public static ITaobaoExecutor GetTaobaoExecutor()
       {
           return new Impl.TaobaoExecutor();
       }
       public static IMobileExecutor GetMobileExecutor()
       {
           return new Impl.MobileExecutor();
       }

       public static IMobileExecutor GetJxlMobileExecutor()
       {
           return new Impl.JxlMobileExecutor();
       }

       public static IProvidentFundExecutor GetProvidentFundExecutor()
       {
           return new Impl.ProvidentFundExecutor();
       }
       public static ISociaSecurityExecutor GetSociaSecurityExecutor()
       {
           return new Impl.SociaSecurityExecutor();
       }
       public static IPbccrcExecutor GetPbccrcExecutor()
       {
           return new Impl.PbccrcExecutor();
       }
       public static IVcreditCertifyExecutor GetVcreditCertifyExecutor()
       {
           return new Impl.VcreditCertifyExecutor();
       }
       public static IChsiExecutor GetChsiExecutor()
       {
           return new Impl.ChsiExecutor();
       }
       public static IJobExecutor GetJobExecutor()
       {
           return new Impl.JobExecutor();
       }

    }
}
