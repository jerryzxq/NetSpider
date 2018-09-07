using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using Vcredit.Common.Utility;

namespace Vcredit.NetSpider.ThirdParty.Minshi
{
    public class MinshiFaceverify
    {
        string faceCompareUrl = "http://www.yitu-test.com/face/v1/application/identity_verification/face_verification";
        string faceUploadUrl = "http://www.yitu-test.com/face/v1/application/identity_verification/user/upload_database_image";

        string accessID = "13006";
        string accessKey = "811fddfcb5ed4bb22f430942e1e6f21a";
        HttpResult httpResult = null;
        HttpHelper httpHelper = new HttpHelper();
        HttpItem httpItem = null;
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
        public MinshiFaceverify()
        { }

        public decimal FaceCompare(string personPhoto, string identityPhoto)
        {
            string user_id = CommonFun.GetGuidID();
            string faceParam = "{\"gender\": -1,"//性别
                              + "\"database_image_type\": 2,"//照片类型
                              + "\"database_image_content\":\"" + identityPhoto+"\","
                              + "\"auto_rotate\":true,"
                              + "\"user_id\": \"" + user_id+"\""//用户ID
                              + "}";

            string md5Param = CommonFun.GetMd5Str(accessID + faceParam + accessKey);
            httpItem = new HttpItem
            {
                URL = faceUploadUrl,
                Method = "POST",
                Postdata = faceParam,
                ContentType = "application/json"
            };
            httpItem.Header.Add("x-access-id", accessID);
            httpItem.Header.Add("x-signature", md5Param);

            httpResult = httpHelper.GetHtml(httpItem);

            faceParam = "{\"user_id\":\"" + user_id + "\",\"query_image_content\":\"" + personPhoto + "\"}";

            md5Param = CommonFun.GetMd5Str(accessID + faceParam + accessKey);
            httpItem = new HttpItem
            {
                URL = faceCompareUrl,
                Method = "POST",
                Postdata = faceParam,
                ContentType = "application/json"
            };
            httpItem.Header.Add("x-access-id", accessID);
            httpItem.Header.Add("x-signature", md5Param);

            httpResult = httpHelper.GetHtml(httpItem);


            decimal compareRusult = jsonService.GetResultFromMultiNode(httpResult.Html, "pair_verify_similarity").ToDecimal(0);
            return compareRusult;
        }
    }
}
