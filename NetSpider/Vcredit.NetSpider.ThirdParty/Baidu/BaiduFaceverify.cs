using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.PluginManager;
using Vcredit.Common.Ext;
using System.IO;
using System.Drawing;


namespace Vcredit.NetSpider.ThirdParty.Baidu
{
    public class BaiduFaceverify
    {
        string faceCompareUrl = " http://apis.baidu.com/idl_baidu/faceverifyservice/face_compare";
        string faceCompareParam = string.Empty;
        string apikey = "fd0b6e8fae97e65dab90f4343fdaa429";
        HttpResult httpResult = null;
        HttpHelper httpHelper = new HttpHelper();
        HttpItem httpItem = null;
        IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
        public BaiduFaceverify()
        { }

        public decimal FaceCompare(string personPhoto, string identityPhoto)
        {
            faceCompareParam = "{\"params\": [{ \"cmdid\":\"1000\", \"appid\": \"" + apikey + "\",\"clientip\":\"10.138.30.1\",\"type\":\"st_groupverify\", \"groupid\": \"wangyi1\",\"versionnum\": \"1.0.0.1\",\"usernames\": {\"name2\": \"name2\",\"name1\": \"name1\"},\"images\": {\"name2\": \"" + personPhoto + "\",\"name1\":\"" + identityPhoto + "\"},\"cates\": { \"name2\": \"2\",\"name1\": \"3\"}}],\"jsonrpc\": \"2.0\",\"method\": \"Compare\",\"id\":0}";
            httpItem = new HttpItem
            {
                URL = faceCompareUrl,
                Method = "POST",
                Postdata = faceCompareParam
            };
            httpItem.Header.Add("apikey", apikey);

            httpResult = httpHelper.GetHtml(httpItem);

            decimal compareRusult = jsonService.GetResultFromMultiNode(httpResult.Html, "result:_ret:reslist:name2|name1").ToDecimal(0);
            return compareRusult;
        }


        #region 商汤人脸识别
        public decimal ShangTangFaceCompare(string personPhoto, string identityPhoto)
        {
            var personName = personPhoto.Split('\\')[3];
            var identityName = identityPhoto.Split('\\')[3];
            string postdata = "-----------358323765";
            postdata += "\r\nContent-Disposition: form-data; name=\"api_id\"";
            postdata += "\r\n\r\n0f97b63773164bcaa67bce1cfad0153a";
            postdata += "\r\n-----------358323765";


            postdata += "\r\nContent-Disposition: form-data; name=\"api_secret\"";
            postdata += "\r\n\r\n3ab51897f4e942fdbe367d1024f879a6";
            postdata += "\r\n-----------358323765";

            postdata += "\r\nContent-Disposition: form-data; name=\"selfie_auto_rotate\"";
            postdata += "\r\n\r\ntrue";
            postdata += "\r\n-----------358323765";

            postdata += "\r\nContent-Disposition: form-data; name=\"historical_selfie_auto_rotate\"";
            postdata += "\r\n\r\ntrue";
            postdata += "\r\n-----------358323765";

            postdata += "\r\nContent-Disposition: form-data; name=\"selfie_file\"; filename=\"" + personName + "\"\r\n";
            postdata += "Content-Type: image/jpg\r\n\r\n";

            byte[] HeadBytes = Encoding.ASCII.GetBytes(postdata.ToString());

            byte[] PicBytes1 = ImageToBytesFromFilePath(personPhoto);

            postdata = "";
            postdata += "\r\n-----------358323765";
            postdata += "\r\nContent-Disposition: form-data; name=\"historical_selfie_file\"; filename=\"" + identityName + "\"\r\n";
            postdata += "Content-Type: image/jpg\r\n\r\n";
            byte[] HeadBytes2 = Encoding.ASCII.GetBytes(postdata.ToString());

            byte[] PicBytes2 = ImageToBytesFromFilePath(identityPhoto);
            postdata = "";
            postdata += "\r\n-----------358323765--\r\n";
            byte[] HeadBytes3 = Encoding.ASCII.GetBytes(postdata.ToString());


            byte[] UploadBuffers = null;
            UploadBuffers = ComposeArrays(HeadBytes, PicBytes1);
            UploadBuffers = ComposeArrays(UploadBuffers, HeadBytes2);
            UploadBuffers = ComposeArrays(UploadBuffers, PicBytes2);
            UploadBuffers = ComposeArrays(UploadBuffers, HeadBytes3);


            httpItem = new HttpItem
            {
                URL = "https://v1-auth-api.visioncloudapi.com/identity/historical_selfie_verification",
                Method = "POST",
                ContentType = "multipart/form-data; boundary=---------358323765",
                //Postdata = postdata
                PostdataByte = UploadBuffers,
                PostDataType = PostDataType.Byte
            };
            httpResult = httpHelper.GetHtml(httpItem);
            var msg = jsonService.GetResultFromParser(httpResult.Html, "status");
            if (msg == "OK")
            {
                decimal compareRusult = jsonService.GetResultFromParser(httpResult.Html, "confidence").ToDecimal(0);
                return compareRusult;
            }
            else
            {
                return -1;
            }


        }

        #endregion


        #region Face++人脸比对

        public decimal FaceIDFaceCompare(string personPhoto, string identityPhoto)
        {
            try
            {
                string facetoken = string.Empty;

                string postdata = "-----------358323765";
                postdata += "\r\nContent-Disposition: form-data; name=\"api_key\"";  //apikey
                postdata += "\r\n\r\nvcredit-test";
                postdata += "\r\n-----------358323765";


                postdata += "\r\nContent-Disposition: form-data; name=\"api_secret\"";  //apisecret
                postdata += "\r\n\r\n1lurMNrd23-0iVqxef4BEnUL44X7lwu5";
                postdata += "\r\n-----------358323765";


                postdata += "\r\nContent-Disposition: form-data; name=\"image\"; filename=\"" + personPhoto + "\"\r\n";  //上传的本人照片
                postdata += "Content-Type: image/jpg\r\n\r\n";

                byte[] HeadBytes = Encoding.ASCII.GetBytes(postdata.ToString());
                byte[] PicBytes1 = ImageToBytesFromFilePath(personPhoto);


                postdata = "";
                postdata += "\r\n-----------358323765--\r\n";
                byte[] HeadBytes2 = Encoding.ASCII.GetBytes(postdata.ToString());


                byte[] UploadBuffers = null;
                UploadBuffers = ComposeArrays(HeadBytes, PicBytes1);
                UploadBuffers = ComposeArrays(UploadBuffers, HeadBytes2);


                //第一步上传本人图片
                faceCompareUrl = "https://api.faceid.com/faceid/v1/detect";
                httpItem = new HttpItem
                {
                    URL = faceCompareUrl,
                    ContentType = "multipart/form-data; boundary=---------358323765",
                    Method = "POST",
                    PostdataByte = UploadBuffers,
                    PostDataType = PostDataType.Byte
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var faces = jsonService.GetArrayFromParse(httpResult.Html, "faces");  //如果照片中包含多张人脸，则按质量分数降序排序，取第一个为质量分数最高上传图片
                if (faces.Count>0)
                {
                    facetoken = jsonService.GetResultFromParser(faces[0], "token");
                }



                //第二步 与身份证照片对比
                postdata = "";
                postdata = "-----------358323765";
                postdata += "\r\nContent-Disposition: form-data; name=\"api_key\"";    //apikey
                postdata += "\r\n\r\nvcredit-test";
                postdata += "\r\n-----------358323765";


                postdata += "\r\nContent-Disposition: form-data; name=\"api_secret\"";  //apisecret
                postdata += "\r\n\r\n1lurMNrd23-0iVqxef4BEnUL44X7lwu5";
                postdata += "\r\n-----------358323765";

                postdata += "\r\nContent-Disposition: form-data; name=\"name\"";  //姓名（可任意设置，需UTF-8编码）
                postdata += "\r\n\r\nxxxyyy";
                postdata += "\r\n-----------358323765";

                postdata += "\r\nContent-Disposition: form-data; name=\"idcard\"";  //身份证号码（可任意设置）
                postdata += "\r\n\r\n422201199301723694";
                postdata += "\r\n-----------358323765";

                postdata += "\r\nContent-Disposition: form-data; name=\"face_token\"";  //上一步上传本人照片后返回的token
                postdata += "\r\n\r\n" + facetoken + "";
                postdata += "\r\n-----------358323765";

                postdata += "\r\nContent-Disposition: form-data; name=\"image_idcard[]\"; filename=\"" + identityPhoto + "\"\r\n";  //上传的身份证照片
                postdata += "Content-Type: image/jpg\r\n\r\n";

                byte[] HeadBytesIDCard = Encoding.ASCII.GetBytes(postdata.ToString());
                byte[] PicBytes1IDCard = ImageToBytesFromFilePath(identityPhoto);

                postdata = "";
                postdata += "\r\n-----------358323765--\r\n";
                byte[] HeadBytes2IDCard = Encoding.ASCII.GetBytes(postdata.ToString());

                UploadBuffers = null;
                UploadBuffers = ComposeArrays(HeadBytesIDCard, PicBytes1IDCard);
                UploadBuffers = ComposeArrays(UploadBuffers, HeadBytes2IDCard);

                faceCompareUrl = "https://api.faceid.com/faceid/v1/verify";
                httpItem = new HttpItem
                {
                    URL = faceCompareUrl,
                    ContentType = "multipart/form-data; boundary=---------358323765",
                    Method = "POST",
                    PostdataByte = UploadBuffers,
                    PostDataType = PostDataType.Byte
                };
                httpResult = httpHelper.GetHtml(httpItem);
                var result_idcard = jsonService.GetResultFromParser(httpResult.Html, "result_idcard[]");
                if (!result_idcard.IsEmpty())
                {
                    decimal compareRusult = jsonService.GetResultFromParser(result_idcard, "confidence").ToDecimal(0);
                    return compareRusult;
                }
                else
                {
                    return -1;
                }
           
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                return -1;
            }
           
        }


        #endregion


        #region 图片转Byte数组
        private byte[] ImageToBytesFromFilePath(string FilePath)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (Image Img = Image.FromFile(FilePath))
                {
                    using (Bitmap Bmp = new Bitmap(Img))
                    {
                        Bmp.Save(ms, Img.RawFormat);
                    }
                }
                return ms.ToArray();
            }
        }
        #endregion

        #region 图片转Byte数组
        public static byte[] ComposeArrays(byte[] Array1, byte[] Array2)
        {
            byte[] Temp = new byte[Array1.Length + Array2.Length];
            Array1.CopyTo(Temp, 0);
            Array2.CopyTo(Temp, Array1.Length);
            return Temp;
        }
        #endregion
    }
}
