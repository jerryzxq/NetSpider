using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Crawler.Mobile;
using Vcredit.NetSpider.DataAccess.Mongo;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.Mobile;
using Vcredit.NetSpider.PluginManager;
using Vcredit.NetSpider.Processor.Operation;
using Vcredit.NetSpider.Service;
using Vcredit.Common.Ext;
using Vcredit.Common.Constants;
using Vcredit.Common;

namespace Vcredit.NetSpider.Processor.Impl
{
    internal class MobileExecutor : IMobileExecutor
    {
        #region 变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        #endregion

        #region 初始化
        public VerCodeRes MobileInit(string mobile)
        {
            if (mobile.IsEmpty())
                return new VerCodeRes() { StatusCode = ServiceConsts.StatusCode_fail, StatusDescription = ServiceConsts.MobileEmpty };
            MobileReq mobileReq = new MobileReq()
            {
                Mobile = mobile
            };
            return Init(mobileReq);
        }

        public VerCodeRes MobileInit(MobileReq mobileReq)
        {
            VerCodeRes Res = new VerCodeRes();
            if (mobileReq.IdentityCard.IsEmpty() || mobileReq.Name.IsEmpty() || mobileReq.Mobile.IsEmpty())
            {
                Res.StatusDescription = ServiceConsts.IdentityCardOrNameOrMobileEmpty;
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                return Res;
            }
            Res = Init(mobileReq);
            return Res;
        }

        #endregion

        #region 登录

        public BaseRes MobileLogin(MobileReq mobileReq)
        {
            BaseRes Res = new BaseRes();
            if (mobileReq.Token.IsEmpty() || mobileReq.IdentityCard.IsEmpty() || mobileReq.Name.IsEmpty() || mobileReq.Mobile.IsEmpty() || mobileReq.Password.IsEmpty())
            {
                Res.StatusDescription = ServiceConsts.TokenOrIdentityCardOrNameOrMobileOrPwdEmpty;
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                return Res;
            }
            int crawlerCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
            IMobileCrawler crawler = CreateMobileCrawlerByWebsite(mobileReq.Website);
            ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();

            //判断是否有最近提交
            Res = crawler.MobileLogin(mobileReq);
            Res.Website = mobileReq.Website;
            crawlerCode = Res.StatusCode;
            Res.StatusCode = Res.StatusCode == ServiceConsts.CrawlerStatusCode_CheckSuccess || Res.StatusCode == ServiceConsts.StatusCode_success ? ServiceConsts.StatusCode_success : ServiceConsts.StatusCode_fail;

            try
            {
                Spd_applyEntity applyEntity = SaveAppLog(mobileReq, Res, crawlerCode);
                if (Res.nextProCode == ServiceConsts.NextProCode_Query || Res.nextProCode == ServiceConsts.NextProCode_ServicePassword)
                {
                    if (Res.StatusCode == ServiceConsts.StatusCode_success)
                    {
                        Res.Result = "{\"Id\":\"" + "vcredit_" + applyEntity.ApplyId + "\"}";
                        //异步保存登录信息
                        var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                applyEntity.Crawl_status = ServiceConsts.CrawlerStatusCode_Crawlering;
                                applyEntity.Description = ServiceConsts.Crawlering;
                                applyService.Update(applyEntity);
                            }
                            catch (Exception e)
                            {
                                Log4netAdapter.WriteError("修改手机:" + applyEntity.Mobile + "时出错", e);
                            }
                            try
                            {
                                Res = crawler.MobileCrawler(mobileReq, applyEntity.CreateTime);
                                applyEntity.Crawl_status = Res.StatusCode == ServiceConsts.StatusCode_success ? ServiceConsts.CrawlerStatusCode_CrawlerSuccess : ServiceConsts.CrawlerStatusCode_CrawlerFail;
                                applyEntity.Apply_status = Res.StatusCode;
                                applyEntity.Description = Res.StatusDescription;
                                applyService.Update(applyEntity);
                            }
                            catch (Exception e)
                            {
                                Log4netAdapter.WriteError("回写手机:" + applyEntity.Mobile + "时出错", e);
                            }
                        });
                        if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted || task.Status == TaskStatus.Canceled)
                            task.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("保存手机申请(" + mobileReq.Mobile + ")时出错", e);
                throw new Exception(e.Message);
            }
            return Res;
        }

        #endregion

        #region 发送和校验短信验证码

        public VerCodeRes MobileSendSms(MobileReq mobileReq)
        {
            VerCodeRes Res = new VerCodeRes();
            if (mobileReq.Mobile.IsEmpty() || mobileReq.Token.IsEmpty())
            {
                Res.StatusDescription = ServiceConsts.TokenOrMobileEmpty;
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                return Res;
            }
            IMobileCrawler crawler = CreateMobileCrawlerByWebsite(mobileReq.Website);
            Res = crawler.MobileSendSms(mobileReq);
            Res.Website = mobileReq.Website;
            Res.StatusCode = Res.StatusCode == ServiceConsts.CrawlerStatusCode_CheckSuccess || Res.StatusCode == ServiceConsts.StatusCode_success ? ServiceConsts.StatusCode_success : ServiceConsts.StatusCode_fail;
            return Res;
        }

        public BaseRes MobileCheckSms(MobileReq mobileReq)
        {
            BaseRes Res = new BaseRes();
            int crawlerCode = ServiceConsts.CrawlerStatusCode_CheckSuccess;
            if (mobileReq.Mobile.IsEmpty() || mobileReq.Token.IsEmpty() || mobileReq.Smscode.IsEmpty())
            {
                Res.StatusDescription = ServiceConsts.TokenOrMobileOrSmscodeEmpty;
                Res.StatusCode = ServiceConsts.StatusCode_fail;
                return Res;
            }
            IMobileCrawler crawler = CreateMobileCrawlerByWebsite(mobileReq.Website);
            ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
            Spd_applyEntity applyEntity = applyService.GetByToken(mobileReq.Token);
            if (applyEntity != null)
            {
                mobileReq.Name = applyEntity.Name;
                mobileReq.IdentityCard = applyEntity.Identitycard;
            }

            Res = crawler.MobileCheckSms(mobileReq);
            Res.Website = mobileReq.Website;
            crawlerCode = Res.StatusCode;
            Res.StatusCode = Res.StatusCode == ServiceConsts.CrawlerStatusCode_CheckSuccess || Res.StatusCode == ServiceConsts.StatusCode_success ? ServiceConsts.StatusCode_success : ServiceConsts.StatusCode_fail;
            applyEntity.Apply_status = Res.StatusCode;
            applyEntity.Description = Res.StatusDescription;
            applyEntity.Crawl_status = crawlerCode;
            applyService.Update(applyEntity);

            if (Res.nextProCode == ServiceConsts.NextProCode_Query)
            {
                if (Res.StatusCode == ServiceConsts.StatusCode_success)
                {
                    Res.Result = "{\"Id\":\"" + "vcredit_" + applyEntity.ApplyId + "\"}";
                    //异步保存登录信息
                    var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            applyEntity.Crawl_status = ServiceConsts.CrawlerStatusCode_Crawlering;
                            applyEntity.Description = ServiceConsts.Crawlering;
                            applyService.Update(applyEntity);
                        }
                        catch (Exception e)
                        {
                            Log4netAdapter.WriteError("修改手机:" + applyEntity.Mobile + "时出错", e);
                        }

                        try
                        {
                            Res = crawler.MobileCrawler(mobileReq, applyEntity.CreateTime);
                            applyEntity.Crawl_status = Res.StatusCode == ServiceConsts.CrawlerStatusCode_CheckSuccess || Res.StatusCode == ServiceConsts.StatusCode_success ? ServiceConsts.CrawlerStatusCode_CrawlerSuccess : ServiceConsts.CrawlerStatusCode_CrawlerFail;
                            applyEntity.Apply_status = Res.StatusCode;
                            applyEntity.Description = Res.StatusDescription;
                            applyService.Update(applyEntity);
                        }
                        catch (Exception e)
                        {
                            Log4netAdapter.WriteError("回写手机:" + applyEntity.Mobile + "时出错", e);
                        }
                    });
                    if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted || task.Status == TaskStatus.Canceled)
                        task.Dispose();
                }
            }

            return Res;
        }

        #endregion

        #region 解析

        /// <summary>
        /// 读取抓取的数据
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        public BaseRes MobileAnalysis(MobileReq mobileReq, DateTime crawlerDate)
        {
            BaseRes Res = new BaseRes();
            IMobileCrawler crawler = CreateMobileCrawlerByWebsite(mobileReq.Website);
            Res = crawler.MobileAnalysis(mobileReq, crawlerDate);
            return Res;
        }

        #endregion

        #region 查询

        public Summary MobileSummaryQuery(MobileReq mobileReq)
        {
            if (mobileReq.Token.IsEmpty() && (mobileReq.IdentityCard.IsEmpty() || mobileReq.Mobile.IsEmpty())) return null;
            var dic = GetNewSource(mobileReq);
            if (dic["sourceType"].ToString() == ServiceConsts.SpiderType_Mobile)
            {
                var apply = (Spd_applyEntity)dic["sourceData"];
                MobileMongo mobileMongo = new MobileMongo(apply.CreateTime);
                return mobileMongo.GetSummaryByToken(apply.Token);
            }
            else
            {
                return new JxlMobileExecutor().MobileSummaryQuery(mobileReq);
            }
        }

        public Variable_mobile_summaryEntity MobileVariableSummary(MobileReq mobileReq)
        {
            if (mobileReq.Token.IsEmpty() && (mobileReq.IdentityCard.IsEmpty() || mobileReq.Mobile.IsEmpty())) return null;
            ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
            IVariable_mobile_summary variableService = NetSpiderFactoryManager.GetVariable_mobile_summaryService();
            if (mobileReq.IdentityCard.IsEmpty() || mobileReq.Mobile.IsEmpty())
            {
                Spd_applyEntity apply = applyService.GetByToken(mobileReq.Token);
                if (apply == null) return null;
                mobileReq.IdentityCard = apply.Identitycard;
                mobileReq.Mobile = apply.Mobile;
            }
            Variable_mobile_summaryEntity entity = variableService.GetByBusIdentityNoAndMobile(mobileReq.IdentityCard, mobileReq.Mobile, mobileReq.BusType);
            return entity;
        }

        public Basic MobileQuery(MobileReq mobileReq)
        {
            if (mobileReq.Token.IsEmpty() && (mobileReq.IdentityCard.IsEmpty() || mobileReq.Mobile.IsEmpty())) return null;
            var dic = GetNewSource(mobileReq);
            if (dic["sourceType"].ToString() == ServiceConsts.SpiderType_Mobile)
            {
                var apply = (Spd_applyEntity)dic["sourceData"];
                MobileMongo mobileMongo = new MobileMongo(apply.CreateTime);
                return mobileMongo.GetBasicByToken(apply.Token);
            }
            else
            {
                return new JxlMobileExecutor().MobileQuery(mobileReq);
            }
        }

        public List<Call> MobileCall(MobileReq mobileReq)
        {
            if (mobileReq.Token.IsEmpty() && (mobileReq.IdentityCard.IsEmpty() || mobileReq.Mobile.IsEmpty())) return null;
            var dic = GetNewSource(mobileReq);
            if (dic["sourceType"].ToString() == ServiceConsts.SpiderType_Mobile)
            {
                var apply = (Spd_applyEntity)dic["sourceData"];
                MobileMongo mobileMongo = new MobileMongo(apply.CreateTime);
                return mobileMongo.GetBasicCallByToken(apply.Token);
            }
            else
            {
                return new JxlMobileExecutor().MobileCall(mobileReq);
            }
        }

        public BaseRes MobileCatName(string mobile)
        {
            BaseRes Res = new BaseRes();
            Res.Website = GetWebsite(mobile);
            Res.StatusCode = 0;
            return Res;
        }

        /// <summary>
        /// 获取采集状态
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns></returns>
        public BaseRes GetCrawlerState(MobileReq mobileReq)
        {
            BaseRes Res = new BaseRes();
            if (mobileReq.Token.IsEmpty() && (mobileReq.IdentityCard.IsEmpty() || mobileReq.Mobile.IsEmpty()))
            {
                Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                Res.StatusDescription = "缺少必填项";
                return Res;
            }
            var dic = GetNewSource(mobileReq);
            if (dic["sourceType"].ToString() == ServiceConsts.SpiderType_Mobile)
            {
                var apply = (Spd_applyEntity)dic["sourceData"];
                if (apply.Crawl_status == ServiceConsts.CrawlerStatusCode_AnalysisSuccess)
                {
                    Res.Result = "{\"Type\":\"mobile\",\"Code\":\"" + ServiceConsts.CrawlerStatusCode_AnalysisSuccess + "\",\"Description\":\"采集成功\"}";
                }
                else if (apply.Crawl_status == ServiceConsts.CrawlerStatusCode_Crawlering
                    || apply.Crawl_status == ServiceConsts.CrawlerStatusCode_CrawlerSuccess
                    || apply.Crawl_status == ServiceConsts.CrawlerStatusCode_Analysising)
                {
                    Res.Result = "{\"Type\":\"mobile\",\"Code\":\"" + ServiceConsts.CrawlerStatusCode_Crawlering + "\",\"Description\":\"采集中\"}";
                }
                else
                {
                    Res.Result = "{\"Type\":\"mobile\",\"Code\":\"" + ServiceConsts.CrawlerStatusCode_AnalysisFail + "\",\"Description\":\"采集失败\"}";
                }
            }
            else if (dic["sourceType"].ToString() == ServiceConsts.SpiderType_JxlMobile)
            {
                var oprLog = (OperationLogEntity)dic["sourceData"];
                if (oprLog != null)
                {
                    if (oprLog.ReceiveFilePath != null)
                        Res.Result = "{\"Type\":\"jxlmobile\",\"Code\":\"" + ServiceConsts.CrawlerStatusCode_AnalysisSuccess + "\",\"Description\":\"解析成功\"}";
                    else if (oprLog.ReceiveFailCount >= 10)
                        Res.Result = "{\"Type\":\"jxlmobile\",\"Code\":\"" + ServiceConsts.CrawlerStatusCode_AnalysisFail + "\",\"Description\":\"解析失败\"}";
                    else
                        Res.Result = "{\"Type\":\"jxlmobile\",\"Code\":\"" + ServiceConsts.CrawlerStatusCode_Crawlering + "\",\"Description\":\"" + ServiceConsts.Crawlering + "\"}";
                }
            }
            else
            {
                Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                Res.StatusDescription = "无此请求";
                return Res;
            }
            Res.StatusCode = ServiceConsts.StatusCode_success;
            return Res;
        }

        /// <summary>
        /// 获取采集记录
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public List<Spd_applyEntity> GetCollectRecords(Dictionary<string, string> dic)
        {
            if (dic == null) return null;
            if (dic["pageIndex"].IsEmpty() || dic["pageSize"].IsEmpty() && (dic["name"].IsEmpty() || dic["mobile"].IsEmpty() || dic["identityCard"].IsEmpty() || dic["token"].IsEmpty())) return null;
            int pageIndex = dic["pageIndex"].ToInt(0);
            int pageSize = dic["pageSize"].ToInt(20);
            dic.Remove("pageIndex");
            dic.Remove("pageSize");
            ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
            List<Spd_applyEntity> applys = applyService.GetApplyPageList(dic, pageIndex, pageSize);
            return applys;
        }

        #region 根据sourceid获取

        /// <summary>
        /// 获取采集状态
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns></returns>
        public BaseRes GetCrawlerState(string source)
        {
            BaseRes Res = new BaseRes();
            ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
            if (source.IsEmpty())
            {
                Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                Res.StatusDescription = "缺少必填项";
                return Res;
            }
            var sources = source.Split('_');
            if (sources.Count() != 2)
            {
                Res.StatusCode = ServiceConsts.StatusCode_httpfail;
                Res.StatusDescription = "格式不正确";
                return Res;
            }
            if (sources[0] == "vcredit")
            {
                Spd_applyEntity apply = applyService.Get(sources[1].ToInt());
                if (apply.Spider_type == ServiceConsts.SpiderType_Mobile)
                {
                    if (apply.Crawl_status == ServiceConsts.CrawlerStatusCode_AnalysisSuccess)
                    {
                        Res.Result = "{\"Type\":\"mobile\",\"Code\":\"" + ServiceConsts.CrawlerStatusCode_AnalysisSuccess + "\",\"Description\":\"采集成功\"}";
                    }
                    else if (apply.Crawl_status == ServiceConsts.CrawlerStatusCode_Crawlering
                        || apply.Crawl_status == ServiceConsts.CrawlerStatusCode_CrawlerSuccess
                        || apply.Crawl_status == ServiceConsts.CrawlerStatusCode_Analysising)
                    {
                        Res.Result = "{\"Type\":\"mobile\",\"Code\":\"" + ServiceConsts.CrawlerStatusCode_Crawlering + "\",\"Description\":\"采集中\"}";
                    }
                    else
                    {
                        Res.Result = "{\"Type\":\"mobile\",\"Code\":\"" + ServiceConsts.CrawlerStatusCode_AnalysisFail + "\",\"Description\":\"采集失败\"}";
                    }
                }
            }
            else
            {
                Res = new JxlMobileExecutor().GetCrawlerState(source);
            }
            Res.StatusCode = ServiceConsts.StatusCode_success;

            return Res;
        }

        public Variable_mobile_summaryEntity MobileVariableSummary(string source)
        {
            if (source.IsEmpty()) return null;
            IVariable_mobile_summary variableService = NetSpiderFactoryManager.GetVariable_mobile_summaryService();
            Variable_mobile_summaryEntity entity = null;
            var sources = source.Split('_');
            if (sources.Count() == 2)
                entity = variableService.GetBySourceIdAndSourceType(sources[1], sources[0]);
            return entity;
        }

        public Basic MobileQuery(string source)
        {
            if (source.IsEmpty()) return null;
            var sources = source.Split('_');
            if (sources[0] == "vcredit")
            {
                ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
                Spd_applyEntity apply = applyService.Get(sources[1].ToInt());
                MobileMongo mobileMongo = new MobileMongo(apply.CreateTime);
                return mobileMongo.GetBasicByToken(apply.Token);
            }
            else
            {
                return new JxlMobileExecutor().MobileQuery(source);
            }
        }

        public List<Call> MobileCall(string source)
        {
            if (source.IsEmpty()) return null;
            var sources = source.Split('_');
            if (sources[0] == "vcredit")
            {
                ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
                Spd_applyEntity apply = applyService.Get(sources[1].ToInt());
                MobileMongo mobileMongo = new MobileMongo(apply.CreateTime);
                return mobileMongo.GetBasicCallByToken(apply.Token);
            }
            else
            {
                return new JxlMobileExecutor().MobileCall(source);
            }
        }

        #endregion

        #endregion

        #region 重置密码

        public VerCodeRes ResetInit(MobileReq mobileReq)
        {
            throw new NotImplementedException();
        }

        public VerCodeRes ResetSendSms(MobileReq mobileReq)
        {
            throw new NotImplementedException();
        }

        public BaseRes ResetPassWord(MobileReq mobileReq)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region 私有方法

        private VerCodeRes Init(MobileReq mobileReq)
        {
            VerCodeRes Res = new VerCodeRes();
            //if (IsHavingMobile(mobileReq, Res)) return Res;
            string website = GetWebsite(mobileReq.Mobile);
            if (website != "")
            {
                IMobileCrawler crawler = CreateMobileCrawlerByWebsite(website);
                if (crawler != null)
                {
                    mobileReq.Website = website;
                    Res = crawler.MobileInit(mobileReq);
                    Res.Website = website;
                }
                else
                    Res.Website = null;

            }
            else
            {
                Res.Website = website;
            }
            return Res;

        }

        /// <summary>
        /// 根据采集网站创建对象
        /// </summary>
        /// <param name="Website">采集网站</param>
        /// <returns></returns>
        private IMobileCrawler CreateMobileCrawlerByWebsite(string Website)
        {
            if (string.IsNullOrEmpty(Website))
            {
                throw new NotImplementedException("采集网站不能为空！");
            }
            EnumMobileCompany enumCom = new EnumMobileCompany();
            string region = string.Empty;
            string[] mobileStr = Website.Split('_');
            switch (mobileStr[0])
            {
                case "ChinaUnicom": region = "unicom"; enumCom = EnumMobileCompany.ChinaUnicom; break;
                case "ChinaMobile": region = mobileStr[1]; enumCom = EnumMobileCompany.ChinaMobile; break;
                case "ChinaNet": region = mobileStr[1]; enumCom = EnumMobileCompany.ChinaNet; break;
                default: break;
            }
            IMobileCrawler crawler = Crawler.CrawlerManager.GetMobileCrawler(enumCom, region);

            return crawler;
        }

        /// <summary>
        /// 获取采集网站
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <returns></returns>
        private string GetWebsite(string mobile)
        {
            string Url = "http://tcc.taobao.com/cc/json/mobile_tel_segment.htm?tel=";
            EnumMobileCompany enumCom = new EnumMobileCompany();
            string catname = string.Empty;
            string region = string.Empty;

            httpItem = new HttpItem()
            {
                URL = Url + mobile
            };
            httpResult = httpHelper.GetHtml(httpItem);
            catname = CommonFun.GetMidStrByRegex(httpResult.Html, "catName:'", "'");
            //if (catname == "中国联通")
            //{
            switch (catname)
            {
                case "中国联通": region = CommonFun.GetProvinceCode(CommonFun.GetMidStrByRegex(httpResult.Html, "province:'", "'")); enumCom = EnumMobileCompany.ChinaUnicom; break;
                case "中国移动": region = CommonFun.GetProvinceCode(CommonFun.GetMidStrByRegex(httpResult.Html, "province:'", "'")); enumCom = EnumMobileCompany.ChinaMobile; break;
                case "中国电信": region = CommonFun.GetProvinceCode(CommonFun.GetMidStrByRegex(httpResult.Html, "province:'", "'")); enumCom = EnumMobileCompany.ChinaNet; break;
                default: break;
            }
            return enumCom + "_" + region;
            //}
            //else
            //{
            //    return "";
            //}
        }

        private bool IsHavingMobile(MobileReq mobileReq, VerCodeRes Res)
        {
            ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
            Spd_applyEntity applyEntity = applyService.GetMobileSpiderByIdentityCardAndMobileAndName(mobileReq.IdentityCard, mobileReq.Name, mobileReq.Mobile);
            if (applyEntity == null) return false;
            if (applyEntity.Crawl_status != ServiceConsts.CrawlerStatusCode_AnalysisSuccess) return false;

            Res.StatusCode = ServiceConsts.StatusCode_success;
            Res.StatusDescription = applyEntity.Description;
            Res.Website = applyEntity.Website;
            Res.Token = applyEntity.Token;
            Res.nextProCode = ServiceConsts.NextProCode_Query;

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    Spd_applyEntity applyEnt = new Spd_applyEntity()
                    {
                        Identitycard = applyEntity.Identitycard,
                        IPAddr = applyEntity.IPAddr,
                        Mobile = applyEntity.Mobile,
                        Name = applyEntity.Name,
                        AppId = mobileReq.BusType,
                        Website = applyEntity.Website,
                        Spider_type = applyEntity.Spider_type,
                        Apply_status = applyEntity.Apply_status,
                        Crawl_status = applyEntity.Crawl_status,
                        Description = applyEntity.Description,
                        Token = applyEntity.Token,
                    };
                    applyService.Save(applyEntity);
                }
                catch (Exception e)
                {
                    Log4netAdapter.WriteError("保存手机用户信息时出错", e);
                }
            });
            return true;
        }

        private bool IeExistMobileWebsite(string website)
        {
            // return true;
            if (website == null) return false;
            var mobilePix = website.Split('_');
            if (mobilePix[0] == "ChinaUnicom") website = mobilePix[0];
            ConfigMongo mongo = new ConfigMongo();
            MobileSeting seting = mongo.GetMobileSeting(website);
            if (seting == null) return false;
            return seting.IsUse == 1;
        }

        private Spd_applyEntity SaveAppLog(MobileReq mobileReq, BaseRes Res, int crawlerCode)
        {
            ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
            Spd_applyEntity applyEntity = applyService.GetByToken(mobileReq.Token);
            if (applyEntity != null)
            {
                applyEntity.Name = mobileReq.Name;
                applyEntity.Identitycard = mobileReq.IdentityCard;
                applyEntity.Apply_status = Res.StatusCode;
                applyEntity.Crawl_status = ServiceConsts.CrawlerStatusCode_CheckSuccess;
                applyEntity.Description = Res.StatusDescription;
                applyService.Update(applyEntity);
            }
            else
            {
                applyEntity = new Spd_applyEntity();
                applyEntity.Identitycard = mobileReq.IdentityCard;
                applyEntity.IPAddr = mobileReq.IPAddr;
                applyEntity.Mobile = mobileReq.Mobile;
                applyEntity.Name = mobileReq.Name;
                applyEntity.AppId = mobileReq.BusType ?? String.Empty;
                applyEntity.Website = mobileReq.Website;
                applyEntity.Spider_type = ServiceConsts.SpiderType_Mobile;
                applyEntity.Apply_status = Res.StatusCode;
                applyEntity.Crawl_status = crawlerCode;
                applyEntity.Description = Res.StatusDescription;
                applyEntity.Token = mobileReq.Token;
                applyEntity.Spd_applyformList.Add(new Spd_applyformEntity() { Form_name = "Password", Form_value = mobileReq.Password });
                applyEntity.ApplyId = applyService.Save(applyEntity).ToString().ToInt().Value;
            }
            return applyEntity;
        }

        /// <summary>
        /// 取最新数据源
        /// </summary>
        /// <param name="apply"></param>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        private static Dictionary<string, object> GetNewSource(MobileReq mobileReq)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            ISpd_apply applyService = NetSpiderFactoryManager.GetSpd_applyService();
            if (!mobileReq.IdentityCard.IsEmpty() && !mobileReq.Mobile.IsEmpty())
            {
                IOperationLog oprService = NetSpiderFactoryManager.GetOperationLogService();
                var applyEntity = applyService.GetByIdentityCardAndMobile(mobileReq.IdentityCard, mobileReq.Mobile);
                var operEntity = oprService.GetByIdentityNoAndMobile(mobileReq.IdentityCard, mobileReq.Mobile);
                if (applyEntity != null && operEntity != null)
                {
                    if (applyEntity.CreateTime >= operEntity.SendTime && applyEntity.Spider_type == ServiceConsts.SpiderType_Mobile)
                    {
                        dic.Add("sourceType", ServiceConsts.SpiderType_Mobile);
                        dic.Add("sourceData", applyEntity);
                    }
                    else
                    {
                        dic.Add("sourceType", ServiceConsts.SpiderType_JxlMobile);
                        dic.Add("sourceData", operEntity);
                    }
                }
                else if (applyEntity != null)
                {
                    dic.Add("sourceType", ServiceConsts.SpiderType_Mobile);
                    dic.Add("sourceData", applyEntity);
                }
                else if (operEntity != null)
                {
                    dic.Add("sourceType", ServiceConsts.SpiderType_JxlMobile);
                    dic.Add("sourceData", operEntity);
                }
            }
            else
            {
                var apply = applyService.GetByToken(mobileReq.Token);
                if (apply != null)
                {
                    mobileReq.IdentityCard = apply.Identitycard;
                    mobileReq.Mobile = apply.Mobile;
                    dic.Add("sourceType", ServiceConsts.SpiderType_Mobile);
                    dic.Add("sourceData", apply);
                }
            }
            return dic;
        }


        #endregion



    }
}
