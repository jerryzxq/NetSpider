using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;

namespace Vcredit.WeiXin.RestService.Operation
{
    public class RCServiceOpr
    {
        string rcFraudUrl = Vcredit.Common.Utility.Chk.IsNull(ConfigurationManager.AppSettings["rcServiceUrl"]);

        /// <summary>
        /// 检查欺诈分是否通过
        /// </summary>
        /// <param name="getCreditInfo"></param>
        /// <returns></returns>
        private bool CheckFraud(string Job,string IdentityNo,string Mobile,string ReportId,string MobileId,string Name)
        {
            try
            {
                string url = rcFraudUrl + "/RCRestService4DDfraud/DoUnifromDecision4DDOffLinefraudLevel/" + IdentityNo;
                string postData = JsonConvert.SerializeObject(new
                {
                    job =Job,
                    star_state = GetStarByIdentityNo(IdentityNo),
                    mobile = Mobile,
                    reportId = ReportId,
                    crdType = 1 , //网络版征信1，机构版征信2
                    mobileId =MobileId,
                    applydate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    callfrom = "统一接口",
                    CustName = Name
                });
                HttpItem httpItem = new HttpItem()
                {
                    Method = "post",
                    URL = url,
                    ContentType = "application/json;charset=utf-8",
                    Postdata = postData
                };

                HttpResult httpResults = new HttpHelper().GetHtml(httpItem);
                string result = httpResults.Html;
                dynamic jsonObj = JsonConvert.DeserializeObject(result);
                if (jsonObj["resultcode"].Value == 0)
                {
                    string level = jsonObj["level"].Value;
                    if (level.Contains("拒绝"))
                        return false;
                }
                else
                {
                    Log4netAdapter.WriteInfo("获取第三方负面失败：" + result);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("获取欺诈分信息失败", ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 根据身份证号获取星座
        /// </summary>
        /// <param name="identityNo"></param>
        /// <returns></returns>
        private string GetStarByIdentityNo(string identityNo)
        {
            string constellation = "";
            try
            {
                string birthDay = Vcredit.Common.Utility.ChinaIDCard.GetBirthDate(identityNo);
                DateTime time = (DateTime)birthDay.ToDateTime(Consts.DateFormatString11);
                int month = time.Month;
                int day = time.Day;

                if ((month == 1 && day < 20) || month == 12 && day > 21)
                    constellation = "摩羯座";
                else if ((month == 1 && day > 19) || (month == 2 && day < 19))
                    constellation = "水瓶座";
                else if ((month == 2 && day > 18) || (month == 3 && day < 21))
                    constellation = "双鱼座";
                else if ((month == 3 && day > 20) || (month == 4 && day < 20))
                    constellation = "白羊座";
                else if ((month == 4 && day > 19) || (month == 5 && day < 21))
                    constellation = "金牛座";
                else if ((month == 5 && day > 20) || (month == 6 && day < 22))
                    constellation = "双子座";
                else if ((month == 6 && day > 21) || (month == 7 && day < 23))
                    constellation = "巨蟹座";
                else if ((month == 7 && day > 22) || (month == 8 && day < 23))
                    constellation = "狮子座";
                else if ((month == 8 && day > 22) || (month == 9 && day < 23))
                    constellation = "处女座";
                else if ((month == 9 && day > 22) || (month == 10 && day < 24))
                    constellation = "天秤座";
                else if ((month == 10 && day > 23) || (month == 11 && day < 23))
                    constellation = "天蝎座";
                else if ((month == 11 && day > 22) || (month == 12 && day < 22))
                    constellation = "射手座";
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError(identityNo + "转换星座出错", e);
            }
            return constellation;
        }
    }
}