using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.NetSpider.Processor.Operation;
using Vcredit.NetSpider.Service;

namespace Vcredit.NetSpider.Processor.Impl
{
    public class VcreditCertifyExecutor : IVcreditCertifyExecutor
    {
        public BaseRes GetVcreditCertify(string sort, string username, string password, params string[] prams)
        {
            BaseRes Res = new BaseRes();
            VcreditCertifyOpr Opr = new VcreditCertifyOpr();
            IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();
            switch (sort)
            {
                case "taobao": Res = Opr.TaobaoCertify(username, password, prams[0]); break;
                case "jd": Res = Opr.JingDongCertify(username, password, prams[0]); break;
                case "chsi": Res = Opr.ChsiCertify(username, password); break;
                case "mail": Res = Opr.MailCertify(username, password); break;
                default: break;
            }
            return Res;
        }
    }
}
