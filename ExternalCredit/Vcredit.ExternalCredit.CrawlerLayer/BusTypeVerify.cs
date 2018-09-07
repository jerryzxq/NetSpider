using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using VVcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExternalCredit.CrawlerLayer
{
    public class CreditType
    {
        /// <summary>
        /// 外贸征信
        /// </summary>
        public const string ExtTrade = "ExtCredit";
        /// <summary>
        /// 担保征信
        /// </summary>
        public const string AssureCredit = "AssureCredit";
        /// <summary>
        /// 成都小贷
        /// </summary>
        public const string ChengDuCredit = "ChengDuCredit";
        /// <summary>
        /// 上海小贷
        /// </summary>
        public const string ShangHaiCredit = "ShangHaiCredit";
    }
    public class BusTypeVerify
    {
         //static NameValueCollection busTypeInfoList;

        public static NameValueCollection BusTypeInfoList
        {
            get
            {
               return GetConfigInfoByNameSpace("OrgCredit.BusType");
            }
        }
        private  static NameValueCollection GetConfigInfoByNameSpace(string nameSpace)
        {
            return  ApolloConfig.GetPropertyNameValueCollection(nameSpace);
        }
        /// <summary>
        /// 检查项目组请求的征信是否合法
        /// </summary>
        /// <param name="busType">产品类别</param>
        /// <param name="namespaces">指定命名空间、私有属性</param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool CheckBusType(string creditType, string busType, string ip = null)
        {
            bool isVerify = false;
            string[] ipArrConfigs = null;//配置ip
            string[] creditTypeConfigs = null;//配置征信类型：成都小贷，上海小贷，外贸征信，担保征信
            var busTypeInfoList = BusTypeInfoList;
            try
            {
                if (string.IsNullOrEmpty(ip))
                    ip = CommonFun.GetClientIP();

                if (string.IsNullOrEmpty(ip))
                    Log4netAdapter.WriteInfo(string.Format("creditType:{0},bustype{1}获取不到ip地址", creditType, busType));
                else
                    Log4netAdapter.WriteInfo(string.Format("creditType:{0},bustype{1},ip:{2}请求信息", creditType, busType, ip));
                if (busTypeInfoList.AllKeys.Contains(busType))
                {
                    var value = busTypeInfoList.Get(busType);
                   var creditTypeAndIpInfo= value.Split('|');
                   if (creditTypeAndIpInfo.Length!=2)//如果分割不是两项，说明配置有问题，直接返回验证失败
                       return isVerify;
                   creditTypeConfigs = creditTypeAndIpInfo[0].Split(',');
                   ipArrConfigs = creditTypeAndIpInfo[1].Split(';');
                   if ((creditTypeConfigs[0]=="*"||creditTypeConfigs.Contains(creditType)) && (ipArrConfigs[0] == "*" || ipArrConfigs.Contains(ip)))
                       isVerify = true;
                }
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError(string.Format("creditType:{0},bustype{1}出现异常", creditType, busType), e);
            }
            return isVerify;
        }

 
    }
}
