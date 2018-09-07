using System;
using System.Collections.Generic;
using System.Net;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Emall.Framework;
using Vcredit.NetSpider.Emall.Entity;
using Vcredit.NetSpider.Emall.Business;
using Vcredit.Framework.Server.Dto;
using Vcredit.NetSpider.Emall.Dto;
using Vcredit.NetSpider.Emall.Crawler.Alipay.CrawlerBill;
using Vcredit.NetSpider.Emall.Crawler.Alipay.Login;
using Vcredit.NetSpider.Emall.Crawler.Alipay;
using Vcredit.NetSpider.Emall.Crawler.Common;
using Vcredit.Common.Ext;
using Vcredit.Framework.Queue.Redis;
namespace Vcredit.NetSpider.Emall.Crawler
{
    public class AlipayCrawler : IEmallCrawler<EmallReq>
    {
        Dictionary<string, string> dyMode = new Dictionary<string, string>();
        CookieCollection cookies = new CookieCollection();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="loginReq"></param>
        /// <returns></returns>
        public VerCodeRes EmallInit(EmallReq loginReq = null)
        {
            VerCodeRes Res = new VerCodeRes();
            try
            {
                var ResToken = CommonFun.GetGuidID();

                #region 初始化
                ///加载初始化
                dyMode = new LoadInit(ResToken).GetResult(ref cookies);

                SetRedisQue(dyMode, ResToken);


                //验证是否需要验证码
                Res = new LoadVerify(ResToken).GetResult(loginReq, ref dyMode, ref cookies);
                Res.Token = ResToken;
                if (loginReq == null || "User" == loginReq.LoginType)
                {
                    dyMode["LoginType"] = "User";
                }
                else if (loginReq != null && "QCode" == loginReq.LoginType)
                {
                    Qrcode qcode = new Qrcode(Res.Token);
                    dyMode["LoginType"] = "QCode";
                    Res = qcode.DoQrcode(dyMode);
                    Res.Token = ResToken;
                }
                #endregion

                RedisHelper.SetCache(Res.Token + "_dy", dyMode);
                RedisHelper.SetCache(Res.Token, cookies);

            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "支付宝初始化异常";
                LogUtility.Write(Res.Token, e, "支付宝初始化异常", SystemConstant.Alipay, "AlipayCrawler.EmallInit");

            }
            return Res;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="loginReq"></param>
        /// <returns></returns>
        public BaseRes EmallLogin(EmallReq loginReq)
        {
            BaseRes Res = new BaseRes();

            try
            {

                cookies = (CookieCollection)RedisHelper.GetCache(loginReq.Token);
                dyMode = (Dictionary<string, string>)RedisHelper.GetCache(loginReq.Token + "_dy");

                //登录
                Res = new Login(loginReq.Token).DoLogin(loginReq, ref dyMode, ref cookies);
                if (Res.StatusCode != (int)SystemEnums.ServiceStatus.Success)
                {
                    RedisHelper.RemoveCookieCache(loginReq.Token);
                    RedisHelper.RemoveCookieCache(loginReq.Token + "_dy");
                }
                else
                {
                    RedisHelper.SetCache(loginReq.Token, cookies);
                    RedisHelper.SetCache(loginReq.Token + "_dy", dyMode);

                }
                return Res;
            }
            catch (Exception ex)
            {
                Res.StatusDescription = "支付宝登录异常";
                Res.StatusCode = (int)SystemEnums.ServiceStatus.Fail;
                LogUtility.Write(loginReq.Token, ex, "支付宝登录异常", SystemConstant.Alipay, "AlipayCrawler.EmallLogin");

                return Res;
            }
        }
        /// <summary>
        /// 验证短息
        /// </summary>
        /// <param name="loginReq"></param>
        /// <returns></returns>
        public BaseRes EmallCheckSMS(EmallReq loginReq)
        {
            BaseRes Res = new BaseRes();
            try
            {

                cookies = (CookieCollection)RedisHelper.GetCache(loginReq.Token);
                dyMode = (Dictionary<string, string>)RedisHelper.GetCache(loginReq.Token + "_dy");

                Res = new CheckSms(loginReq.Token).DoCheckSms(loginReq, ref dyMode, ref cookies, loginReq.Token);

                RedisHelper.SetCache(loginReq.Token, cookies);
                RedisHelper.SetCache(loginReq.Token + "_dy", dyMode);

                return Res;
            }
            catch (Exception ex)
            {
                Res.StatusDescription = "支付宝短信验证异常";
                Res.StatusCode = (int)SystemEnums.ServiceStatus.Fail;
                LogUtility.Write(loginReq.Token, ex, "支付宝短信验证异常", SystemConstant.Alipay, "AlipayCrawler.EmallCheckSMS");

                return Res;
            }
        }

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="loginReq"></param>
        /// <returns></returns>
        public BaseRes EmallCrawler(EmallReq loginReq)
        {
            BaseRes Res = new BaseRes();
            try
            {


                cookies = (CookieCollection)RedisHelper.GetCache(loginReq.Token);
                dyMode = (Dictionary<string, string>)RedisHelper.GetCache(loginReq.Token + "_dy");

                AlipayBaicEntity baic = new AlipayBaicEntity();

                #region 基本信息
                //个人详情
                baic = new BaicCrawler(loginReq.Token).DoBaic(loginReq, ref dyMode, ref cookies);
                if (baic == null) throw new Exception("支付宝基本资料为空");
                AlipayBaicBll.Initialize.Save(baic);
                if (!loginReq.BackUrl.IsEmpty())
                    EmallCommon.CrawlerCallBack(loginReq.BackUrl, loginReq.Token, SystemConstant.Alipay, EmallCrawlerStep.Baic);
                #endregion

                #region  花呗订单

                var flowers = new FlowerBillCrawler(loginReq.Token).DoBillFlower(ref cookies, dyMode, baic);
                if (flowers != null)
                    baic.FlowerBill.AddRange(flowers);

                baic.SaveAction<AlipayBillFlowerEntity>(baic.FlowerBill, AlipayBillFlowerBll.Initialize.ActionSave);
                if (!loginReq.BackUrl.IsEmpty())
                    EmallCommon.CrawlerCallBack(loginReq.BackUrl, loginReq.Token, SystemConstant.Alipay, EmallCrawlerStep.HuaBei);
                #endregion

                #region 银行卡
                var bank = new BackCrawler(loginReq.Token).DoBank(dyMode, ref cookies);
                if (bank != null)
                    baic.Bank.AddRange(bank);
                #endregion

                #region  地址

                var addresslist = new BackCrawler(loginReq.Token).DoAddress(ref cookies);
                if (addresslist != null)
                    baic.Address.AddRange(addresslist);

                #endregion

                #region  电子账单
                //var electron = new ElectronicBillCrawler().DoElectronicBill(ref cookies);
                //if (electron != null)
                //    baic.ElectronicBill.AddRange(electron);
                #endregion

                #region 保存
                AlipayBaicBll.Initialize.CrawlerSave(baic);
                #endregion


                #region  账单

                AddBillCrawlerRedisQueue(baic);
                #endregion

                #region 发送Kafka 数据
                AliPaySendData.SendKafka(baic);
                #endregion


                #region  登出

                ///登出
                //new Login().DoLoginOut(cookies);

                #endregion

            }
            catch (Exception ex)
            {
                Res.StatusDescription = "支付宝采集异常";
                Res.StatusCode = (int)SystemEnums.ServiceStatus.Fail;
                LogUtility.Write(loginReq.Token, ex, "支付宝采集异常", SystemConstant.Alipay, "AlipayCrawler.EmallCrawler");
                return Res;
            }
            Res.StatusDescription = "支付宝采集成功";
            Res.StatusCode = ServiceConsts.StatusCode_success;
            return Res;
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="loginReq"></param>
        /// <returns></returns>
        public BaseRes EmalSendSMS(EmallReq loginReq)
        {
            BaseRes Res = new BaseRes();
            try
            {
                //获取缓存

                cookies = (CookieCollection)RedisHelper.GetCache(loginReq.Token);
                dyMode = (Dictionary<string, string>)RedisHelper.GetCache(loginReq.Token + "_dy");

                Res = new SendSms(loginReq.Token).DoLoginSendSms(dyMode, ref cookies);

                RedisHelper.SetCache(loginReq.Token, cookies);
                RedisHelper.SetCache(loginReq.Token + "_dy", dyMode);

                return Res;
            }
            catch (Exception ex)
            {
                Res.StatusDescription = "支付宝短信发送异常";
                Res.StatusCode = (int)SystemEnums.ServiceStatus.Fail;
                LogUtility.Write(loginReq.Token, ex, "支付宝短信发送异常", SystemConstant.Alipay, "AlipayCrawler.EmalSendSMS");

                return Res;
            }
        }

        /// <summary>
        /// 传入Cookie登录
        /// </summary>
        /// <param name="loginReq"></param>
        /// <returns></returns>
        public BaseRes EmallLoginForCookies(EmallReq loginReq)
        {
            BaseRes Res = new BaseRes();
            try
            {

                //个人详情
                Res = new Login(loginReq.Token).DoLoginForCookies(loginReq, dyMode, ref cookies);
                RedisHelper.SetCache(loginReq.Token, cookies);
                RedisHelper.SetCache(loginReq.Token + "_dy", dyMode);
                RedisHelper.SetCache(loginReq.Token + "_cookie", loginReq.Cookies);
                return Res;
            }
            catch (Exception ex)
            {
                Res.StatusDescription = "支付宝登录异常";
                Res.StatusCode = (int)SystemEnums.ServiceStatus.Fail;
                LogUtility.Write(loginReq.Token, ex, "支付宝登录异常", SystemConstant.Alipay, "AlipayCrawler.EmallLoginForCookies");

                return Res;
            }
        }


        public bool IsCookieEffective(EmallReq req)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取二维码操作
        /// </summary>
        /// <param name="loginReq"></param>
        /// <returns></returns>
        public VerCodeRes EmallQRCode(EmallReq loginReq)
        {

            VerCodeRes Res = new VerCodeRes();
            try
            {
                //获取缓存

                cookies = (CookieCollection)RedisHelper.GetCache(loginReq.Token);
                dyMode = (Dictionary<string, string>)RedisHelper.GetCache(loginReq.Token + "_dy");


                Res = new Qrcode(loginReq.Token).DoQrcode(dyMode);
                RedisHelper.SetCache(loginReq.Token, cookies);
                RedisHelper.SetCache(loginReq.Token + "_dy", dyMode);

                return Res;
            }
            catch (Exception ex)
            {
                Res.StatusDescription = "获取二维码操作异常";
                Res.StatusCode = (int)SystemEnums.ServiceStatus.Fail;
                LogUtility.Write(loginReq.Token, ex, "获取二维码操作异常", SystemConstant.Alipay, "AlipayCrawler.EmallQRCode");

                return Res;
            }
        }

        public void SetRedisQue(Dictionary<string, string> dyMode, string token)
        {
            if (dyMode.Count > 0)
            {
                AliPayWinServiceRes res = new AliPayWinServiceRes()
                {
                    Token = token,
                    ResToken = dyMode["_json_token"],
                    LoanType = 1
                };

                RedisQueue.Send(res, "AliPayWinUaService");
            }
        }

        public void AddBillCrawlerRedisQueue(AlipayBaicEntity baic)
        {
            if (baic != null)
            {
                AliPayBillBaic obj = new AliPayBillBaic()
                {
                    ID = baic.ID,
                    Token = baic.Token,
                    UserID = baic.UserID
                };

                Vcredit.NetSpider.Cache.WorkQueue.Enqueue<AliPayBillBaic>(obj, "Queue:AliPayBillCrawlerQueue");
            }
        }
        /// <summary>
        /// 验证二维码
        /// </summary>
        /// <param name="loginReq"></param>
        /// <returns></returns>
        public BaseRes EmallCheckQRCode(EmallReq loginReq)
        {
            BaseRes Res = new BaseRes();
            try
            {
                //获取缓存

                cookies = (CookieCollection)RedisHelper.GetCache(loginReq.Token);
                dyMode = (Dictionary<string, string>)RedisHelper.GetCache(loginReq.Token + "_dy");

                Res = new Qrcode(loginReq.Token).QueryCodeSecurityId(dyMode, cookies, loginReq.Token);
                RedisHelper.SetCache(loginReq.Token, cookies);
                RedisHelper.SetCache(loginReq.Token + "_dy", dyMode);

                return Res;
            }
            catch (Exception ex)
            {
                Res.StatusDescription = "验证二维码异常";
                Res.StatusCode = (int)SystemEnums.ServiceStatus.Fail;
                LogUtility.Write(loginReq.Token, ex, "验证二维码异常", SystemConstant.Alipay, "AlipayCrawler.EmallCheckQRCode");

                return Res;
            }
        }
    }
}
