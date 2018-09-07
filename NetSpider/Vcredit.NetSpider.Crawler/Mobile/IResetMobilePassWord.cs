using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.Mobile;

namespace Vcredit.NetSpider.Crawler.Mobile
{
    public interface IResetMobilePassWord
    {

        /// <summary>
        /// 初始化页面
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        VerCodeRes ResetInit(MobileReq mobileReq);

        /// <summary>
        /// 发送重置密码
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        VerCodeRes ResetSendSms(MobileReq mobileReq);

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        BaseRes ResetPassWord(MobileReq mobileReq);
    }
}
