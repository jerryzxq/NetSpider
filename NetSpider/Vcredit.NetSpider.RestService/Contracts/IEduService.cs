using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.Chsi;

namespace Vcredit.NetSpider.RestService.Contracts
{
    [ServiceContract]
    interface IEduService
    {
        #region 学信网接口
        [WebInvoke(UriTemplate = "/chsi/register/1/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("学信网注册第一步")]
        VerCodeRes ChsiRegisterStep1ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/chsi/register/1/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("学信网注册第一步")]
        VerCodeRes ChsiRegisterStep1ForJson(Stream stream);

        [WebInvoke(UriTemplate = "/chsi/register/2/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("学信网注册第二步")]
        BaseRes ChsiRegisterStep2ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/chsi/register/2/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("学信网注册第二步")]
        BaseRes ChsiRegisterStep2ForJson(Stream stream);

        [WebInvoke(UriTemplate = "/chsi/register/3/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("学信网注册第三步")]
        BaseRes ChsiRegisterStep3ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/chsi/register/3/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("学信网注册第三步")]
        BaseRes ChsiRegisterStep3ForJson(Stream stream);


        [WebGet(UriTemplate = "/chsi/query/init/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("学信网查询登录页初始化")]
        VerCodeRes ChsiQueryInitForXml();

        [WebGet(UriTemplate = "/chsi/query/init/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("学信网查询登录页初始化")]
        VerCodeRes ChsiQueryInitForJson();

        [WebInvoke(UriTemplate = "/chsi/query/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("学信网查询")]
        BaseRes ChsiQueryForXml(Stream stream);

        [WebInvoke(UriTemplate = "/chsi/query/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("学信网查询")]
        BaseRes ChsiQueryForJson(Stream stream);


        [WebInvoke(UriTemplate = "/chsi/forget/username/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("学信网忘记用户名")]
        BaseRes ChsiForgetUsernameForXml(Stream stream);

        [WebInvoke(UriTemplate = "/chsi/forget/username/json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("学信网忘记用户名")]
        BaseRes ChsiForgetUsernameForJson(Stream stream);


        [WebGet(UriTemplate = "/chsi/forget/password/1/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        [Description("学信网忘记密码，第一步")]
        VerCodeRes ChsiForgetPasswordStep1ForXml();

        [WebGet(UriTemplate = "/chsi/forget/password/1/json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("学信网忘记密码，第一步")]
        VerCodeRes ChsiForgetPasswordStep1ForJson();

        [WebInvoke(UriTemplate = "/chsi/forget/password/2/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("学信网忘记密码，第二步")]
        VerCodeRes ChsiForgetPasswordStep2ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/chsi/forget/password/2/json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("学信网忘记密码，第二步")]
        VerCodeRes ChsiForgetPasswordStep2ForJson(Stream stream);

        [WebInvoke(UriTemplate = "/chsi/forget/password/3/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("学信网忘记密码，第三步")]
        BaseRes ChsiForgetPasswordStep3ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/chsi/forget/password/3/json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("学信网忘记密码，第三步")]
        BaseRes ChsiForgetPasswordStep3ForJson(Stream stream);

        [WebInvoke(UriTemplate = "/chsi/forget/password/4/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("学信网忘记密码，第四步")]
        BaseRes ChsiForgetPasswordStep4ForXml(Stream stream);

        [WebInvoke(UriTemplate = "/chsi/forget/password/4/json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("学信网忘记密码，第四步")]
        BaseRes ChsiForgetPasswordStep4ForJson(Stream stream);
        #endregion

    }
}
