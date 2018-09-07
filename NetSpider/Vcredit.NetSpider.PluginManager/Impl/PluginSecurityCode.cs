using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.PluginManager.Impl
{
    public class PluginSecurityCode : IPluginSecurityCode
    {
        public string GetVerCode(byte[] VerBytes)
        {
            return GetVerCodeCommon(VerBytes, CharSort.All, null);
        }

        public string GetVerCodeByCharSort(byte[] VerBytes, CharSort CharSort)
        {
            return GetVerCodeCommon(VerBytes, CharSort, null);
        }

        private string GetVerCodeCommon(byte[] VerBytes, CharSort Sort, string CharList)
        {
            string srcCode = string.Empty;
            try
            {
                //VercodeWebService.VercodeService vSer = new VercodeWebService.VercodeService();
                //vSer.Timeout = 10000;
                //srcCode = vSer.RecognizeCodeByBytes(VerBytes, 0, (int)Sort);
            }
            catch (Exception e)
            {
                Vcredit.Common.Utility.Log4netAdapter.WriteError("验证码转换出错", e);
            }
            return srcCode;
        }


        public string GetVerCodeFromUUwise(byte[] VerBytes, int CodeType)
        {
            string srcCode = string.Empty;
            try
            {
                //VercodeWebService.VercodeService vSer = new VercodeWebService.VercodeService();
                //srcCode = vSer.RecognizeCodeByBytes(VerBytes, 1, CodeType);
            }
            catch (Exception e)
            {
                Vcredit.Common.Utility.Log4netAdapter.WriteError("验证码转换出错", e);
            }
            return srcCode;
        }
    }
}
