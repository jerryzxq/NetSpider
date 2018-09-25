using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Routing;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.Dto;
using Vcredit.ActivexLogin.FrameWork;
using Vcredit.ActivexLogin.Processor;
using Vcredit.ActivexLogin.WebApi.Filters;

namespace Vcredit.ActivexLogin.WebApi.Controllers
{
    /// <summary>
    /// ActivexLoginController
    /// </summary>
    [RoutePrefix("api/Activex")]
    [LogParamFilter]
    public class ActivexController : ApiController
    {
        public ActivexController()
        {
            ServicePointManager.ServerCertificateValidationCallback =
  ((sender, certificate, chain, sslPolicyErrors) => true);
        }

        private IActivexLoginExecutor excu;

        /// <summary>
        /// 发送原始加密信息，等待加密
        /// </summary>
        /// <returns></returns>
        [Route("SendOriginalData")]
        [HttpPost]
        public dynamic SendOriginalData(ActivexLoginReq req)
        {
            var result = new BaseRes();
            if (!ModelState.IsValid)
            {
                result.StatusCode = Dto.StatusCode.Fail;
                result.StatusDescription = GetModoleStateString();
                return result;
            }

            excu = ProcessorFactory.GenerateExecutorV2(req.SiteType);

            try
            {
                result = excu.SendOriginalData(req);
            }
            catch (Exception ex)
            {
                result.StatusCode = Dto.StatusCode.Fail;
                result.StatusDescription = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 获取加密后信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="siteType"></param>
        /// <returns></returns>
        [Route("GetEncryptData")]
        [HttpGet]
        public dynamic GetEncryptData(string token, ProjectEnums.WebSiteType siteType)
        {
            excu = ProcessorFactory.GenerateExecutorV2(siteType);

            var result = excu.GetEncryptData(token);

            return result;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        [Route("Init")]
        [HttpGet]
        public dynamic Init(ProjectEnums.WebSiteType siteType)
        {
            excu = ProcessorFactory.GenerateExecutorV2(siteType);
            var result = excu.Init();

            return result;
        }

        /// <summary>
        /// 实际登录
        /// </summary>
        /// <param name="siteType"></param>
        /// <param name="token"></param>
        /// <param name="captcha"></param>
        /// <returns></returns>
        [Route("DoRealLogin")]
        [HttpGet]
        public dynamic DoRealLogin(ProjectEnums.WebSiteType siteType, string token, string captcha)
        {
            excu = ProcessorFactory.GenerateExecutorV2(siteType);

            var result = excu.DoRealLogin(token, captcha);

            return result;
        }


        private string GetModoleStateString()
        {
            var message = string.Join(" | ", ModelState.Values
                           .SelectMany(v => v.Errors)
                           .Select(e => e.ErrorMessage));

            return message;
        }

        /// <summary>
        /// 刷新验证码
        /// </summary>
        /// <param name="siteType"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("RefreshCaptcha")]
        public dynamic RefreshCaptcha(ProjectEnums.WebSiteType siteType, string token)
        {
            excu = ProcessorFactory.GenerateExecutorV2(siteType);

            var result = excu.RefreshCaptcha(token);

            return result;
        }

        /////
        ///// 发送原始加密信息，等待加密
        ///// </summary>
        ///// <returns></returns>
        //[Route("TestSendOriginalData")]
        //[HttpPost]
        //public dynamic TestSendOriginalData(ActivexLoginReq req)
        //{
        //    RedisHelper.Enqueue(JsonConvert.SerializeObject(req), WebSiteConstant.CommonRequestPackage);
        //    return true;
        //}

    }

}