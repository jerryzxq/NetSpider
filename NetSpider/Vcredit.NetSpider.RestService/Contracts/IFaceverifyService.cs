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
    interface IFaceverifyService
    {
        [WebInvoke(UriTemplate = "/compare/personandidcard/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("人脸识别证件照与生活照对比")]
        BaseRes FaceCompareByIdcardForXml(Stream stream);

        [WebInvoke(UriTemplate = "/compare/personandidcard/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("人脸识别证件照与生活照对比")]
        BaseRes FaceCompareByIdcardForJson(Stream stream);

        [WebInvoke(UriTemplate = "/compare/getsimilarity/Xml", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, Method = "POST")]
        [Description("人脸识别证件照与生活照对比")]
        BaseRes FaceCompareGetSimilarityForXml(Stream stream);

        [WebInvoke(UriTemplate = "/compare/getsimilarity/Json", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        [Description("人脸识别证件照与生活照对比")]
        BaseRes FaceCompareGetSimilarityForJson(Stream stream);
    }
}
