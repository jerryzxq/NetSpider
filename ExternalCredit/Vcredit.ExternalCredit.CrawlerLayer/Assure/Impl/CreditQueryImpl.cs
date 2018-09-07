using System;
using Newtonsoft.Json;
using System.IO;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.ExtTrade.CommonLayer.helper;
using System.Web;
using Vcredit.Framework.Queue.Redis;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
using Vcredit.ExternalCredit.CrawlerLayer.Assure.AnalisisHtml;
using Vcredit.ExternalCredit.CommonLayer.helper;
using Vcredit.ExternalCredit.Dto;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.Common.Ext;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading;
using Vcredit.ExtTrade.BusinessLayer;
using System.Text;
using Vcredit.ExtTrade.ModelLayer.Common;
using System.Configuration;
using System.Collections.Generic;
using ServiceStack.OrmLite;
using Vcredit.ExternalCredit.CrawlerLayer.Assure.Handlers;
using Vcredit.ExternalCredit.Dto.Assure;

namespace Vcredit.ExternalCredit.CrawlerLayer.Assure
{
    /// <summary>
    /// 征信查询
    /// </summary>
    public class CreditQueryImpl : CreditQuery
    {
        #region Properties

        private CRD_CD_CreditUserInfoBusiness _creditUserInfoBiz = new CRD_CD_CreditUserInfoBusiness();

        #endregion

        public CreditQueryImpl()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
   ((sender, certificate, chain, sslPolicyErrors) => true);
        }

        #region 登陆
        /// <summary>
        /// 登陆
        /// </summary>
        /// <returns></returns>
        public ApiResultDto<AssureLoginResultDto> Login()
        {
            var result = new ApiResultDto<AssureLoginResultDto> { StatusCode = StatusCode.Fail };

            var cookieHandlerRes = new GetCookieHandler().HandleRequest(new HandleRequest());
            var isGetSuccess = cookieHandlerRes.DoNextHandler;
            if (!isGetSuccess)
            {
                result.StatusDescription = "登陆失败了，暂时无法查询数据，请检查登陆接口是否正常";
                Log4netAdapter.WriteInfo(result.StatusDescription);
                return result;
            }
            result.StatusCode = StatusCode.Success;
            result.StatusDescription = "登陆成功";
            result.Result = new AssureLoginResultDto { Cookies = cookieHandlerRes.Data };

            return result;
        }

        #endregion

        #region 提交查询信息
        /// <summary>
        /// 用户信息查询，这里暂不做实际查询，存储数据库通过Job去查询（由于1分钟只能5次查询）
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public ApiResultDto<AddQueryInfoResultDto> AddQueryInfo(AssureQueryUserInfoParamDto param)
        {
            var result = new ApiResultDto<AddQueryInfoResultDto>
            {
                StatusCode = StatusCode.Fail,
                Result = new AddQueryInfoResultDto(),
            };

            //// 时间限制，时间限制外无法操作
            //bool canOpt = new TimeLimitHandler().HandleRequest(new HandleRequest()).DoNextHandler;
            //if (!canOpt)
            //{
            //    result.Result.FailReason = SysEnums.AssureFailReasonState.超出时间限制;
            //    result.StatusDescription = "时间限制外无法操作";
            //    //return new ApiResultDto<string> { StatusCode = StatusCode.Fail, StatusDescription = "时间限制外无法操作！" };
            //}
            //else
            {
                var entity = new CRD_CD_CreditUserInfoEntity()
                {
                    ExpiryDate_Num = ConfigData.upLoadLimitedDays,
                    Authorization_Date = DateTime.Now.ToString("yyyyMMdd"),
                    Name = param.Name,
                    Cert_Type = param.CertType,
                    Cert_No = param.CertNo,
                    State = (int)RequestState.Default,
                    Time_Stamp = DateTime.Now,
                    QueryReason = param.QueryReason,
                    SourceType = (int)SysEnums.SourceType.Assure,
                    LocalDirectoryName = DateTime.Now.ToString("yyyyMMdd"),
                    Error_Reason = "正在等待查询",
                    BusType = param.BusType,

                    ApplyID = param.ApplyID,
                };

                if (_creditUserInfoBiz.Insert(entity))
                {
                    result.StatusCode = StatusCode.Success;
                    result.Result.FailReason = SysEnums.AssureFailReasonState.默认;
                    result.StatusDescription = "数据保存成功，正在等待查询！";
                    //return new ApiResultDto<string> { StatusCode = StatusCode.Success, StatusDescription = "数据保存成功，正在等待查询！" };
                }
                else
                {
                    result.StatusCode = StatusCode.Fail;
                    result.Result.FailReason = SysEnums.AssureFailReasonState.数据保存失败;
                    result.StatusDescription = "数据保存失败！";
                    //return new ApiResultDto<string> { StatusCode = StatusCode.Fail, StatusDescription = "数据保存失败！" };
                }
            }

            return result;
        }
        #endregion

        #region 数据提交
        public bool CommitToQueryV2()
        {
            try
            {
                Console.WriteLine("开始提交");
                Log4netAdapter.WriteInfo("开始提交");

                BaseCreditQueryHandler tmHandler = new TimeLimitHandler();
                BaseCreditQueryHandler commitUserInfoHandler = new NeedCommitUserInfoHandler();
                BaseCreditQueryHandler cookiesHandler = new GetCookieHandler();
                BaseCreditQueryHandler commitHandler = new DoCommitHandler();

                tmHandler.SetNextHandler(commitUserInfoHandler);
                commitUserInfoHandler.SetNextHandler(cookiesHandler);
                cookiesHandler.SetNextHandler(commitHandler);

                var req = new HandleRequest();
                var res = tmHandler.HandleRequest(req);

                this.DoHandelError(req, res);

                Console.WriteLine("结束提交");
                Log4netAdapter.WriteInfo("结束提交");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("提交出现异常，Exception：{0}", ex.Message));
                Log4netAdapter.WriteError("提交出现异常", ex);
                return false;
            }
        }

        private void DoHandelError(HandleRequest req, HandleResponse res)
        {
            // 如果是登陆失败，将当前的数据状态还原（RequestState.UpLoading ==> RequestState.Default）
            if (req.UserInfoList != null && req.UserInfoList.Any() &&
                res.ErrorReason != null && res.ErrorReason == HandlerErrorReason.CookieError)
            {
                // 修改为数据上传中状态更改
                foreach (var uinfo in req.UserInfoList)
                {
                    uinfo.State = (int)RequestState.Default;
                    uinfo.Error_Reason = "登陆失败，等待重新提交";
                    uinfo.UpdateTime = DateTime.Now;
                    _creditUserInfoBiz.UpdateEntity(uinfo);
                }
            }
        }

        #endregion

        #region 下载征信

        public bool AnanysisDownloadCreditV2()
        {
            var result = false;
            try
            {
                Console.WriteLine("开始下载解析");
                Log4netAdapter.WriteInfo("开始下载解析");

                BaseCreditQueryHandler tmHandler = new TimeLimitHandler();
                BaseCreditQueryHandler needDownloadUserInfoHandler = new NeedDownloadUserInfoHandler();
                BaseCreditQueryHandler cookiesHandler = new GetCookieHandler();
                BaseCreditQueryHandler ananysisHandler = new DoAnanysisDownloadHandler();

                tmHandler.SetNextHandler(needDownloadUserInfoHandler);
                needDownloadUserInfoHandler.SetNextHandler(cookiesHandler);
                cookiesHandler.SetNextHandler(ananysisHandler);

                var res = tmHandler.HandleRequest(new HandleRequest());

                Console.WriteLine("结束下载解析");
                Log4netAdapter.WriteInfo("结束下载解析");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("征信信息下载失败，下载或解析异常！Exception：{0}", ex.Message));
                Log4netAdapter.WriteError("征信信息下载失败，下载或解析异常！", ex);
            }

            return result;
        }

        /// <summary>
        /// 单独解析HTML 这里可以测试单独的html解析数据入库
        /// </summary>
        public static void AnanysisCreditByHtml()
        {
            BaseDao dao = new BaseDao();
            // 状态为上传成功 RequestState.UpLoadSuccess 的数据
            // 尝试下载这些数据的征信

            var certNosStr = ConfigurationManager.AppSettings["certNos"];
            Log4netAdapter.WriteInfo("待处理文件：" + certNosStr);

            var certNos = certNosStr.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var certNo in certNos)
            {
                var entity = dao.Select<CRD_CD_CreditUserInfoEntity>(x => x.Cert_No == (certNo)
                                                                        ).OrderByDescending(x => x.CreditUserInfo_Id)
                                                                        .ToList()
                                                                        .FirstOrDefault();
                if (entity == null)
                {
                    Log4netAdapter.WriteInfo(string.Format("{0} 数据库实体不存在", certNo));
                    continue;
                }

                var filePath = Path.Combine(ConfigurationManager.AppSettings["htmlPath"]
                                                , string.Format(@"{0}.html", certNo)
                                            );
                if (!File.Exists(filePath))
                {
                    Log4netAdapter.WriteInfo(string.Format("{0} html文件不存在", certNo));
                    continue;
                }

                var byteArray = File.ReadAllBytes(filePath);
                try
                {
                    // todo 解析征信报告
                    string str = Encoding.Default.GetString(byteArray);
                    new Analisis().SaveData(entity.Cert_No, str);

                    // 状态更改
                    entity.State = (int)RequestState.SuccessCome;
                    entity.Error_Reason = "成功获取征信报告";

                    //todo 获取报告编号
                    entity.Report_sn = CommonFun.GetMidStr(str, "报告编号:", "</font>");

                    // 获取报告取得结果时间
                    entity.UpdateTime = DateTime.Now;

                    Log4netAdapter.WriteInfo(string.Format("证件号为：{0} 成功获取征信报告！", entity.Cert_No));
                }
                catch (Exception ex)
                {
                    // 状态更改
                    entity.State = (int)RequestState.AnalysisFail;
                    entity.Error_Reason = "征信报告解析失败";
                    entity.UpdateTime = DateTime.Now;

                    var msg = string.Format("证件号为：{0} 征信报告解析失败！", entity.Cert_No);
                    Log4netAdapter.WriteError(msg, ex);
                }
                dao.Update(entity);
            }
        }

        #endregion

    }
}
