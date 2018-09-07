using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Cn.Vcredit.VBS.Model;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.WeiXin.RestService.Models;

namespace Vcredit.WeiXin.RestService.Contracts
{
    [ServiceContract]
    interface IBankService
    {
        [WebGet(UriTemplate = "/bankcard/match/{bankname}/{cardNo}/{cardtype}/Json", ResponseFormat = WebMessageFormat.Json)]
        [Description("返回的银行信息是否匹配的结果)")]
        BaseRes MatchBankCard(string bankName, string cardNo, string cardType);

        [WebInvoke(UriTemplate = "/querycard/yinlian/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("查询卡信息")]
        CheckCardService.ResponseResult QueryCardInfoByYinlian(Stream stream);
    }
}
