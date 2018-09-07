using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.Chsi;

namespace Vcredit.NetSpider.Crawler.Edu
{
    public interface IChsiCrawler
    {
        /// <summary>
        /// 学信数据查询初始化
        /// </summary>
        /// <returns></returns>
        VerCodeRes Query_Init();
        /// <summary>
        /// 学信数据查询
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        BaseRes Query_GetInfo(LoginReq login);
        /// <summary>
        /// 学信注册，第一步，初始化
        /// </summary>
        /// <param name="registerReq"></param>
        /// <returns></returns>
        VerCodeRes Register_Init(ChsiRegisterReq registerReq);
        /// <summary>
        /// 学信注册，第二步，发短信验证码
        /// </summary>
        /// <param name="registerReq"></param>
        /// <returns></returns>
        BaseRes Register_Step1(ChsiRegisterReq registerReq);
        /// <summary>
        /// 学信注册，第三步，提交
        /// </summary>
        /// <param name="registerReq"></param>
        /// <returns></returns>
        BaseRes Register_Step2(ChsiRegisterReq registerReq);
        /// <summary>
        /// 找回用户名
        /// </summary>
        /// <param name="forgetReq"></param>
        /// <returns></returns>
        BaseRes ForgetUsername(ChsiForgetReq forgetReq);
        /// <summary>
        /// 找回密码，第一步
        /// </summary>
        /// <returns></returns>
        VerCodeRes ForgetPwd_Step1();

        /// <summary>
        /// 找回密码，第二步
        /// </summary>
        /// <returns></returns>
        VerCodeRes ForgetPwd_Step2(ChsiForgetReq forgetReq);
        /// <summary>
        /// 找回密码，第三步
        /// </summary>
        /// <returns></returns>
        BaseRes ForgetPwd_Step3(ChsiForgetReq forgetReq);
        /// <summary>
        /// 找回密码，第四步
        /// </summary>
        /// <returns></returns>
        BaseRes ForgetPwd_Step4(ChsiForgetReq forgetReq);

        Chsi_InfoEntity Query_GetJxlInfo(string infos);
    }
}
