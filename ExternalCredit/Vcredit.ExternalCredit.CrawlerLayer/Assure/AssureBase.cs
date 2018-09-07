using Newtonsoft.Json;
using NSoup;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CommonLayer.helper;
using Vcredit.ExternalCredit.Dto;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.CommonLayer.helper;

namespace Vcredit.ExternalCredit.CrawlerLayer.Assure
{
    public class AssureBase
    {
        public AssureBase()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
  ((sender, certificate, chain, sslPolicyErrors) => true);
        }

        protected HttpHelper _httpHelper = new HttpHelper();
        protected HttpItem _httpItem;
        protected HttpResult _httpResult;

        /// <summary>
        /// 每次数据库取的数量
        /// </summary>
        protected static int _PreReportCount
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["PreReportCount"].ToInt(100);
                }
                catch (Exception)
                {
                    return 500;
                }
            }
        }


        /// <summary>
        /// 操作唯一 id
        /// </summary>
        protected int _goperationid = 0;

        protected dynamic _currentEntity = null;

        protected BaseDao dao = new BaseDao();

        protected RealityinkeepEntity _realityinkeep = null;

        protected string _cookieToken;

        /// <summary>
        /// 当前登陆cookies
        /// </summary>
        protected string _currentCookies = string.Empty;

        /// <summary>
        /// 登陆名
        /// </summary>
        protected string _loginName
        {
            get
            {
                return ConfigurationManager.AppSettings["loginName"];
            }
        }

        /// <summary>
        /// 密码
        /// </summary>
        private string _password
        {
            get
            {
                return ConfigurationManager.AppSettings["password"];
            }
        }

        private string _redisPackage
        {
            get
            {
                var _packege = ConfigurationManager.AppSettings["redisPackage"];
                if (string.IsNullOrEmpty(_packege))
                    throw new ArgumentException("请在配置文件中添加 redisPackage", "redisPackage");

                return _packege;
            }
        }

        /// <summary>
        /// 判断是否锁定  ireportstate == 4 || recordoprtypeofinfo == 4
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        protected bool IsLock(int ireportstate, int recordoprtypeofinfo)
        {
            return (ireportstate == 4 || recordoprtypeofinfo == 4);
        }

        /// <summary>
        /// 登陆 cookies 是否成功
        /// </summary>
        /// <returns></returns>
        protected bool CookiesIsInited()
        {
            if (_cookieToken.IsEmpty())
                throw new ArgumentException("请初始化 _cookieToken 参数", "_cookieToken");

            _currentCookies = RedisHelper.GetCache<string>(_cookieToken, _redisPackage);
            if (!string.IsNullOrEmpty(_currentCookies))
            {
                // 判断redis缓存_cookies 是否有效
                var url = "https://msi.pbccrc.org.cn/start?r=705";

                _httpItem = new HttpItem
                {
                    URL = url,
                    Cookie = _currentCookies,
                };
                _httpResult = _httpHelper.GetHtml(_httpItem);
                var doc = NSoupClient.Parse(_httpResult.Html);
                if (doc.GetElementById("susername") != null &&
                    doc.GetElementById("spassword") != null)
                {
                    Log4netAdapter.WriteInfo(string.Format("cookies 已失效正在尝试重新登陆！"));
                    TryLogin();
                }

                // todo _cookies 有效重新添加到redis 
            }
            else
            {
                TryLogin();
            }
            if (string.IsNullOrEmpty(_currentCookies))
            {
                Log4netAdapter.WriteInfo(string.Format("无法获取 cookies"));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 尝试登陆
        /// </summary>
        private ApiResultDto<AssureLoginResultDto> TryLogin()
        {
            int tryCount = 1;
            ApiResultDto<AssureLoginResultDto> result = null;
            while (true)
            {
                result = this.LoginMethod();
                _currentCookies = (result.IsSuccess && result.Result != null && !string.IsNullOrEmpty(result.Result.Cookies))
                        ? result.Result.Cookies : string.Empty;

                if (!string.IsNullOrEmpty(_currentCookies))
                {
                    Log4netAdapter.WriteInfo(string.Format("登陆成功"));
                    break;
                }

                // 最多重试5次
                tryCount++;
                if (tryCount > ConfigData.tryLoginCount)
                {
                    Log4netAdapter.WriteInfo(string.Format("{0}次登陆尝试没有成功，请检查登陆是否存在问题！", ConfigData.tryLoginCount));
                    break;
                }
                Log4netAdapter.WriteInfo(string.Format("登陆没有成功，Message：{0}，尝试第{1}次登陆", result.StatusDescription, tryCount));
            }
            return result;
        }

        /// <summary>
        /// 登陆
        /// </summary>
        /// <returns></returns>
        private ApiResultDto<AssureLoginResultDto> LoginMethod()
        {
            var apiResult = new ApiResultDto<AssureLoginResultDto>();
            // 1. 获取初始化_cookies
            var url = "https://msi.pbccrc.org.cn/html/login.html";
            _httpItem = new HttpItem
            {
                URL = url,
                Method = "GET",
            };
            _httpResult = _httpHelper.GetHtml(_httpItem);
            _currentCookies = CookieHelper.ConvertResponseCookieToRequestCookie(_httpResult.Cookie);
            //Log4netAdapter.WriteInfo(string.Format("初始化_cookies：{0}", _cookies));

            // 2. 获取验证码，并且合并_cookies
            var svcode = string.Empty;

            // 四位验证码
            var tryCount = 1;
            while (svcode.Length != 4)
            {
                if (tryCount > 3)
                {
                    apiResult.StatusDescription = "3次尝试解析验证码失败了！";
                    Log4netAdapter.WriteInfo(apiResult.StatusDescription);
                    return (apiResult);
                }

                svcode = this.GetVerifyCode();
                tryCount++;
            }

            this.LoginRequest(svcode, _currentCookies, apiResult);
            return (apiResult);
        }
        /// <summary>
        /// 登陆请求
        /// </summary>
        /// <param name="svcode"></param>
        /// <param name="_cookies"></param>
        /// <param name="apiResult"></param>
        /// <returns></returns>
        private void LoginRequest(string svcode, string _cookies, ApiResultDto<AssureLoginResultDto> apiResult)
        {
            var message = string.Empty;
            // 登陆
            var userName = this._loginName;
            var password = this._password;

            var postdata = string.Format(@"susername={0}&spassword={1}&svcode={2}&ukeyid=", userName, password, svcode);

            var url = "https://msi.pbccrc.org.cn/login";
            _httpItem = new HttpItem
            {
                URL = url,
                Method = "POST",
                Postdata = postdata,
                Cookie = _cookies,
            };
            _httpResult = _httpHelper.GetHtml(_httpItem);
            var rst = JsonConvert.DeserializeObject<AssureLoginResult>(_httpResult.Html);

            //// TODO 测试不登陆
            //var rst = new loginResult();
            if (rst.success)
            {
                if (rst.errors == "1")
                {
                    message = ("机构未递交网签授权协议，请尽快申请。");
                }
                else if (rst.errors == "2")
                {
                    //TODO 需要获取证书格式
                }
                else if (rst.errors == "3")
                {
                    message = ("超过授权时间，请对用户重新进行授权。");
                }
                else if (rst.errors == "4")
                {
                    message = ("机构尚未进行网签，请授权用户及时进行操作。");
                }
                else if (rst.errors == "5")
                {
                    // 登陆成功
                    apiResult.StatusCode = StatusCode.Success;
                    apiResult.Result = new AssureLoginResultDto { Token = _cookieToken, Cookies = _cookies };
                    message = "恭喜你，登陆成功啦！！！";

                    RedisHelper.SetCache(_cookieToken, _cookies, _redisPackage, -1);
                }
            }
            else
            {
                var errors = rst.errors;
                message = (errors) + "，请重试！！！";
            }
            apiResult.StatusDescription = message;
        }
        /// <summary>
        /// 解析获取验证码
        /// </summary>
        /// <returns></returns>
        private string GetVerifyCode()
        {
            _httpItem = new HttpItem
            {
                URL = "https://msi.pbccrc.org.cn/verificationcode?ver=" + DateTime.Now.ToString("yyyy-MM-ddHH:mm:ss"),
                ResultType = ResultType.Byte,
                Cookie = _currentCookies,
            };

            var vcode = string.Empty;
            _httpResult = _httpHelper.GetHtml(_httpItem);
            _currentCookies = CookieHelper.CookieCombine(_currentCookies, CookieHelper.ConvertResponseCookieToRequestCookie(_httpResult.Cookie));
            if (_httpResult.ResultByte != null && _httpResult.ResultByte.Length > 0)
            {
                // 解析验证码
                vcode = AnalysisVCode.GetVerifycode(_httpResult.ResultByte);
            }

            return vcode;
        }

        /// <summary>
        /// 最后执行
        /// </summary>
        protected void DoFinal()
        {
            this.DoUploadSuccess();
            var message = string.Format("担保业务编号： {0} ，上报成功",
                                                _currentEntity.GuaranteeLetterCode
                                    );
            Log4netAdapter.WriteInfo(message);
        }

        /// <summary>
        /// 上传成功
        /// </summary>
        protected void DoUploadSuccess()
        {
            _currentEntity.State = (int)CommonLayer.SysEnums.AssureReportState.UpLoadSuccess;
            _currentEntity.StateDescription = "上报成功";
            _currentEntity.UpdateTime = DateTime.Now;

            _currentEntity.Ukey = _loginName;

            dao.Save(_currentEntity);
        }

        /// <summary>
        /// 上传失败
        /// </summary>
        protected void DoUploadFail(string message)
        {
            _currentEntity.State = (int)CommonLayer.SysEnums.AssureReportState.UpLoadFail;
            _currentEntity.StateDescription = message.Length > 500 ? message.Substring(0, 496) + "..." : message;
            _currentEntity.UpdateTime = DateTime.Now;
            dao.Save(_currentEntity);
        }

        /// <summary>
        /// 重新提交
        /// </summary>
        /// <param name="message"></param>
        protected void DoNeedReUpload(string message)
        {
            _currentEntity.State = (int)CommonLayer.SysEnums.AssureReportState.NeedReUpload;
            _currentEntity.StateDescription = "处理出现异常，等待重新上报... —— " + message;
            _currentEntity.UpdateTime = DateTime.Now;
            dao.Save(_currentEntity);
        }

        /// <summary>
        /// StepAction
        /// </summary>
        /// <param name="action"></param>
        /// <param name="stepName"></param>
        /// <returns></returns>
        protected bool DoStepAction(Action action, RequestStep step)
        {
            var isDoSuccess = false;
            try
            {
                action();

                try
                {
                    var entity = JsonConvert.DeserializeObject<StepResultEntity>(_httpResult.Html);
                    if (!entity.success)
                    {
                        var message = string.Format("担保业务编号： {0} ，方法名称： {1}， 请求失败，接口返回html： {2} ",
                                                                        _currentEntity.GuaranteeLetterCode,
                                                                        step.ToString(),
                                                                        _httpResult.Html
                                                                );
                        this.DoUploadFail(message);


                        Log4netAdapter.WriteInfo(message);
                    }
                    else
                    {
                        isDoSuccess = true; // 请求成功

                        var message = string.Format("担保业务编号： {0} ，方法名称： {1}， 请求成功，接口返回html： {2} ",
                                                                        _currentEntity.GuaranteeLetterCode,
                                                                        step.ToString(),
                                                                        _httpResult.Html
                                                                );
                        Log4netAdapter.WriteInfo(message);
                    }
                }
                catch (Exception ex)
                {
                    var message = string.Format("担保业务编号： {0} ，方法名称： {1}， 处理异常，返回Html：{2}",
                                                           _currentEntity.GuaranteeLetterCode,
                                                           step.ToString(),
                                                           _httpResult.Html
                                                       );
                    this.DoUploadFail(message);

                    Log4netAdapter.WriteError(message, ex);
                }
            }
            catch (Exception ex)
            {
                var message = string.Format("担保业务编号： {0} ，方法名称： {1}， 请求异常", _currentEntity.GuaranteeLetterCode, step.ToString());
                Log4netAdapter.WriteError(message, ex);

                this.DoUploadFail(message);
            }
            return isDoSuccess;
        }



        #region RemoveGbRealityInKeepParam

        protected class RemoveGbRealityInKeepParam
        {
            public GbrealityinkeepParam GbRealityInKeep { get; set; }
        }

        #endregion

        #region GbrealityinkeepParam
        protected class GbrealityinkeepParam
        {
            public int iid { get; set; }
            public string guaranteecontractstatus { get; set; }
            public object guaranteerelievedate { get; set; }
            public string inkeepbalance { get; set; }
            public string balancechangedate { get; set; }
            public int goperationid { get; set; }
            public string oproccurdate { get; set; }
            public string recordoprtypeofinfo { get; set; }
            public string validityflag { get; set; }
            public string validityvarydate { get; set; }
            public long dgetdate { get; set; }
            public string ireportstate { get; set; }
            public object id { get; set; }
        }
        #endregion

        #region RequestStep
        protected enum RequestStep
        {
            StepOne = 1,
            StepTwo = 2,
            StepThree = 3,
            StepFour = 4,
            StepFive = 5,
            StepSix = 6,

            StepDelete = 7
        }
        #endregion

        #region StepResultEntity

        /// <summary>
        /// 接口返回结果实体
        /// </summary>
        protected class StepResultEntity
        {
            public string total { get; set; }
            public bool success { get; set; }
            public string errors { get; set; }
        }
        #endregion

        #region RealityinkeepObject
        protected class RealityinkeepObject
        {
            public int total { get; set; }
            public RealityinkeepEntity[] root { get; set; }
            public bool success { get; set; }
        }

        protected class RealityinkeepEntity
        {
            public int iid { get; set; }
            public long dgetdate { get; set; }
            public int ireportstate { get; set; }
            public string goperationid { get; set; }
            public string oproccurdate { get; set; }
            public int recordoprtypeofinfo { get; set; }
            public string guaranteecontractstatus { get; set; }
            public string validityflag { get; set; }
            public string validityvarydate { get; set; }
            public string guaranteerelievedate { get; set; }
            public string inkeepbalance { get; set; }
            public string balancechangedate { get; set; }
            public string action { get; set; }
            public string columnnameoproccurdate { get; set; }
            public string start { get; set; }
            public string limit { get; set; }
        }


        #endregion

        #region BalanceTransferParam

        protected class BalanceTransferParam
        {
            public Gbbalancetransfer GbBalanceTransfer { get; set; }
            public Gbbalancetransferdetail GbBalanceTransferDetail { get; set; }
        }

        protected class Gbbalancetransfer
        {
            public string subrogationflag { get; set; }
            public int addupbalancetransfersum { get; set; }
            public int assumebalancetransfersum { get; set; }
            public int balancetransferbalance { get; set; }
            public int assumebalancetransferbalance { get; set; }
            public int addupsubrogationsum { get; set; }
            public int adduplosssum { get; set; }
            public string balancetransferdate { get; set; }
            public string balancetransfersum { get; set; }
            public string keepaccountsdate { get; set; }
            public string recentbalancetransferdate { get; set; }
            public int goperationid { get; set; }
            public string oproccurdate { get; set; }
        }

        protected class Gbbalancetransferdetail
        {
            public string balancetransferdate { get; set; }
            public string balancetransfersum { get; set; }
            public int goperationid { get; set; }
            public string oproccurdate { get; set; }
        }

        #endregion
    }
}
