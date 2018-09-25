using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ActivexLogin.Attributes;
using Vcredit.ActivexLogin.Common;
using Vcredit.ActivexLogin.Dto;
using Vcredit.ActivexLogin.FrameWork;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;

namespace Vcredit.ActivexLogin.Processor
{
    public abstract class ExecutorTemplate
    {
        protected HttpHelper httpHelper = new HttpHelper();

        protected string redisPackage;
        protected string redisCookiesPackage;
        protected string redisQueuePackage;
        protected string redisEncryptPackage;

        protected string currentCookie = "";

        public ExecutorTemplate()
        {
            InitParam();
        }

        private void InitParam()
        {
            redisPackage = ((this.GetType().GetCustomAttribute(typeof(RestraintSiteAttribute)) as RestraintSiteAttribute).TargetWebSite).ToString();

            redisQueuePackage = redisPackage + Constants.REQEUST_QUEUE_PREFIX;
            redisEncryptPackage = redisPackage + Constants.REQEUST_ENCRYPT_PREFIX;
            redisCookiesPackage = redisPackage + Constants.REQEUST_COOKIE_PREFIX;
        }

        public virtual BaseRes GetEncryptData(string token)
        {
            if (redisEncryptPackage.IsEmpty())
                throw new ArgumentException("请初始化 redisEncryptPackage");

            var res = new BaseRes() { Token = token };
            var encryptStr = RedisHelper.GetCache<String>(token, redisEncryptPackage);
            if (string.IsNullOrEmpty(encryptStr))
            {
                res.Result = JsonConvert.SerializeObject(
                    new EncryptDataResultDto
                    {
                        Reason = EncryptStatus.NotFoundEncrypt,
                        ReasonDescription = "加密没有完成（加密尚未开始 OR 加密失败 OR 当前Token无效）"
                    });
            }
            else
            {
                res.Result = JsonConvert.SerializeObject(
                    new EncryptDataResultDto
                    {
                        Reason = EncryptStatus.Success,
                        ReasonDescription = "加密成功",
                        Data = encryptStr
                    });
            }

            return res;
        }
    }
}
