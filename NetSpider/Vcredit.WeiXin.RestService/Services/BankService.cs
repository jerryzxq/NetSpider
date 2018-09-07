using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading;
using System.Web;
//using System.Web.Providers.Entities;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.WorkFlow;
using Vcredit.NetSpider.Processor;
using Vcredit.WeiXin.RestService.Contracts;
using Vcredit.Common.Ext;
using System.Data;
using Vcredit.Common;
using Vcredit.NetSpider.PluginManager;
using System.Xml.Linq;
using Vcredit.WeiXin.RestService.Models;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Service;
using Cn.Vcredit.VBS.Model;
using Vcredit.NetSpider.Entity;
using Cn.Vcredit.VBS.BusinessLogic;
using Cn.Vcredit.VBS.BusinessLogic.Entity;
using Vcredit.Common.Helper;
using Vcredit.Common.Constants;
using Vcredit.WeiXin.RestService.Operation;
using Cn.Vcredit.VBS.Interface;
using Cn.Vcredit.VBS.BLL;
using System.Drawing;
using Cn.Vcredit.VBS.PostLoan.FinanceConfig.Action;
using Cn.Vcredit.VBS.PostLoan.OrderInfo;
using Vcredit.NetSpider.Entity.Service.Chsi;
using Vcredit.NetSpider.DataAccess.Ftp;

namespace Vcredit.WeiXin.RestService.Services
{
    //Asp.net管道兼容
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall /*, MaxItemsInObjectGraph = 10000*/)]
    public class BankService : IBankService
    {
        #region 声明变量、接口
        IPbccrcExecutor pbccrcExecutor = ExecutorManager.GetPbccrcExecutor();//社保数据采集接口
        IProvidentFundExecutor profundExecutor = ExecutorManager.GetProvidentFundExecutor();//社保数据采集接口
        ISociaSecurityExecutor socialsecExecutor = ExecutorManager.GetSociaSecurityExecutor();//社保数据采集接口
        IChsiExecutor chsiExecutor = ExecutorManager.GetChsiExecutor();

        IPluginSecurityCode secService = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析接口
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
        string ftpDirectory = "/jxlreport";
        bool isbase64 = true;
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public BankService()
        {
        }
        #endregion

        /// <summary>
        /// 根据银行名称、卡号、卡种匹配银行信息
        /// </summary>
        /// <param name="bankName"></param>
        /// <param name="cardNo"></param>
        /// <param name="cardType"></param>
        /// <returns></returns>
        public BaseRes MatchBankCard(string bankName, string cardNo, string cardType)
        {
            Log4netAdapter.WriteInfo("接口：MatchBankCard；客户端IP:" + CommonFun.GetClientIP());
            BaseRes baseRes = new BaseRes();
            try
            {
                BankOpr opr = new BankOpr();
                baseRes.Result = opr.MatchBankCard(bankName, cardNo, cardType).ToString();
                baseRes.StatusCode = ServiceConsts.StatusCode_success;
                baseRes.StatusDescription = "银行卡匹配接口调用成功";
            }
            catch (Exception e)
            {
                baseRes.StatusCode = ServiceConsts.StatusCode_error;
                baseRes.StatusDescription = "银行卡匹配接口调用异常";
            }
            baseRes.EndTime = DateTime.Now.ToString();
            return baseRes;
        }

        /// <summary>
        /// 通过银联接口，查询卡相关信息
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public CheckCardService.ResponseResult QueryCardInfoByYinlian(Stream stream)
        {
            Log4netAdapter.WriteInfo("接口：QueryCardInfo；客户端IP:" + CommonFun.GetClientIP());
            CheckCardService.ResponseResult result = new CheckCardService.ResponseResult();
            try
            {
                //获得请求内容
                string reqText = stream.AsStringText(true);
                Log4netAdapter.WriteInfo("参数：" + reqText);

                CheckCardService.CheckCardInfo cardInfo = jsonService.DeserializeObject<CheckCardService.CheckCardInfo>(reqText);
                cardInfo.CheckCardCode = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                cardInfo.CreateTime = DateTime.Now;

                //初始化贷后服务
                CheckCardService.CheckCardService service = new CheckCardService.CheckCardService();
                result = service.CheckBankCard(cardInfo);
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("调用贷后银联查询银行卡接口异常", e);
            }
            return result;
        }
    }
}