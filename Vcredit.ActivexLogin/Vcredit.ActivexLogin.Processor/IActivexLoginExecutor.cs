using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ActivexLogin.Dto;

namespace Vcredit.ActivexLogin.Processor
{
    public interface IActivexLoginExecutor
    {
        BaseRes SendOriginalData(ActivexLoginReq req);

        BaseRes GetEncryptData(string token);

        VerCodeRes Init();

        VerCodeRes RefreshCaptcha(string token);

        BaseRes DoRealLogin(string token, string captcha);

    }
}
