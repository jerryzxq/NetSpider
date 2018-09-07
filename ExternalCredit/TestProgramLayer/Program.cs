using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
using Vcredit.ExtTrade.CommonLayer;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using Vcredit.Common.Helper;
using Vcredit.Common;
using Vcredit.Common.Ext;
using NSoup;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade;
using Vcredit.ExternalCredit.CrawlerLayer.JinCityBank;
using Vcredit.ExtTrade.BusinessLayer.CommonBusiness;
using ServiceStack.OrmLite;
using Newtonsoft.Json.Converters;
using Vcredit.ExternalCredit.CrawlerLayer.Assure.AnalisisHtml;
using Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade;
using Vcredit.ExternalCredit.CrawlerLayer.CreditVariable;
using Vcredit.Common.Utility;
using Vcredit.ExtTrade.ModelLayer.Nolmal.NewForeignTradeModel;
using System.Xml.Serialization;
using Vcredit.ExtTrade.ModelLayer.Common;
using Vcredit.ExternalCredit.CrawlerLayer.ShanghaiLoan;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using Vcredit.ExternalCredit.Services.Impl;
using Vcredit.ExternalCredit.Dto;
using System.Net;
using Vcredit.ExternalCredit.CommonLayer.Extension;
using Vcredit.ExternalCredit.Dto.OrgCreditModel;
using Vcredit.ExternalCredit.CommonLayer;
using PetaPoco;
using Vcredit.ExternalCredit.CrawlerLayer;
namespace TestProgramLayer
{
    [Serializable]
    public class Call_LogEntity
    {
        //[Ignore]
        //public  List<Call_ParamEntity>  paramList{ get; set; }
        //[Ignore]
        //public  List<Call_ResultEntity> resultList { get; set; }
        public string ModuleName { get; set; }
        public string IdentityNo { get; set; }
        public string CustName { get; set; }
        public string Result { get; set; }
        public string CallFrom { get; set; }
    }
        class UserFilesRespone
        {
            public bool IsSucceed { get; set; }
            public string Message { get; set; }
            public string Content { get; set; }
        }
        class UserFileContent
        {
            public string IdCardImg { get; set; }
            public string InfoPowerSignatured { get; set; }
            public string InfoPowerSignaturedFileType { get; set; }
        }
        class checkInfo
        {
            public string Name { get; set; }
            public string CertNO { get; set; }
            public DateTime checkTime { get; set; }
            public override string ToString()
            {
                return string.Format("{0},{1},{2}", Name, CertNO, checkTime.ToString());
            }
        }
        class checkEq : IEqualityComparer<checkInfo>
        {

            public bool Equals(checkInfo x, checkInfo y)
            {
                if (x.CertNO == y.CertNO && y.checkTime == x.checkTime)
                    return true;
                return false;
            }

            public int GetHashCode(checkInfo obj)
            {
                return obj.ToString().GetHashCode();
            }
        }

        class reportInfo
        {
            public decimal ReportID { get; set; }
            public string ReportSn { get; set; }
            public DateTime ReportCreateTime { get; set; }
        }
        class stateInfo
        {
            public int State { get; set; }
            public string ErrorReason { get; set; }
            public string ReportSn { get; set; }
        }
        class Models
        {
            List<CRD_HD_REPORTEntity> CRD_HD_REPORTList;

            public List<CRD_HD_REPORTEntity> CRD_HD_REPORTList1
            {
                get { return CRD_HD_REPORTList; }
                set { CRD_HD_REPORTList = value; }
            }
            List<CRD_PI_IDENTITYEntity> CRD_PI_IDENTITYList;

            public List<CRD_PI_IDENTITYEntity> CRD_PI_IDENTITYList1
            {
                get { return CRD_PI_IDENTITYList; }
                set { CRD_PI_IDENTITYList = value; }
            }
        }
        class Model
        {
            CRD_HD_REPORTEntity report;

            public CRD_HD_REPORTEntity Report
            {
                get { return report; }
                set { report = value; }
            }
            CRD_PI_IDENTITYEntity identity;

            public CRD_PI_IDENTITYEntity Identity
            {
                get { return identity; }
                set { identity = value; }
            }

            List<CRD_PI_RESIDENCEEntity> cRD_PI_RESIDENC;

            public List<CRD_PI_RESIDENCEEntity> CRD_PI_RESIDENC
            {
                get { return cRD_PI_RESIDENC; }
                set { cRD_PI_RESIDENC = value; }
            }

            List<CRD_IS_CREDITCUEEntity> cRD_IS_CREDITCUE;

            public List<CRD_IS_CREDITCUEEntity> CRD_IS_CREDITCUE
            {
                get { return cRD_IS_CREDITCUE; }
                set { cRD_IS_CREDITCUE = value; }
            }


        }
        class ErrorData
        {
            string cert_NO;

            public string Cert_NO
            {
                get { return cert_NO; }
                set { cert_NO = value; }
            }
            string cert_Type;

            public string Cert_Type
            {
                get { return cert_Type; }
                set { cert_Type = value; }
            }
            string errorCode;

            public string ErrorCode
            {
                get { return errorCode; }
                set { errorCode = value; }
            }
            string errorDes;

            public string ErrorDes
            {
                get { return errorDes; }
                set { errorDes = value; }
            }
        }
        class Message
        {
            List<Dictionary<string, object>> creditData;

            public List<Dictionary<string, object>> CreditDataList
            {
                get { return creditData; }
                set { creditData = value; }
            }

            List<ErrorData> errorData;

            public List<ErrorData> ErrorDataList
            {
                get { return errorData; }
                set { errorData = value; }
            }
        }
        class Program
        {
            static HttpResult PostDataToUrlGet(string url)
            {
                HttpItem httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "Get"

                };
                return new HttpHelper().GetHtml(httpItem);

            }
            static HttpResult PostDataToUrlGetByCookie(string url, CookieCollection cookies)
            {
                HttpItem httpItem = new HttpItem()
                {
                    ResultCookieType = Vcredit.Common.Helper.ResultCookieType.CookieCollection,
                    CookieCollection = cookies,
                    URL = url,
                    Method = "Get"

                };
                return new HttpHelper().GetHtml(httpItem);

            }
            static HttpResult PostDataToUrlGetByCookie1(string url, CookieCollection cookies)
            {
                HttpItem httpItem = new HttpItem()
                {

                    CookieCollection = cookies,
                    URL = url,
                    Method = "Get",
                    ResultCookieType = ResultCookieType.CookieCollection,
                    Accept = "*/*",
                    Referer = "http://9.80.35.95:8092/zhengxin_gr/index.jsp",
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E)",
                    Host = "9.80.35.95:8092",
                    KeepAlive = true

                };
                return new HttpHelper().GetHtml(httpItem);

            }
            /// <summary>
            /// 获取文件夹名称
            /// </summary>
            /// <returns></returns>
            static string GEtDirectoryName()
            {
                string DirectoryName = string.Empty;
                int hourPoint = 20;
                int.TryParse(ConfigData.todayDealingPoint, out  hourPoint);
                DateTime dtime = DateTime.Now;
                if (dtime.TimeOfDay.Hours >= hourPoint)
                {
                    DirectoryName = dtime.AddDays(1).ToString("yyyyMMdd");
                }
                else
                {
                    DirectoryName = dtime.ToString("yyyyMMdd");
                }
                return DirectoryName;
            }

            static void tesreport()
            {

                BridgingBusiness b = new BridgingBusiness();
                BaseDao dao = new BaseDao();
                try
                {
                    dao.Insert<CRD_HD_REPORTEntity>(new CRD_HD_REPORTEntity()
                    {
                        Report_Sn = "sdfdsf",
                        Cert_No = "sdf",
                        Bh = 0,
                        ImportTime = DateTime.Now,
                        Time_Stamp = DateTime.Now,
                        Query_Time = DateTime.Now,
                        Report_Create_Time = DateTime.Now,

                    });
                }
                catch (Exception)
                {

                    throw;
                }

            }


            private static byte GetOverdue_Cyc(string payment_state)
            {
                if (string.IsNullOrEmpty(payment_state))
                    return 0;
                return (byte)Regex.Matches(payment_state, "[1-7]").Count;

            }
            static string PostDataToUrl(string url, string data)
            {
                HttpItem httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Postdata = data,
                    Encoding = Encoding.UTF8,
                    PostEncoding = Encoding.UTF8,
                    ContentType = "application/json"
                };
                HttpResult httpResult = new HttpHelper().GetHtml(httpItem);
                return httpResult.Html;
            }
            static string PostDataToUrl(string url)
            {
                HttpItem httpItem = new HttpItem()
                {
                    URL = url,
                    Method = "POST"
                };
                HttpResult httpResult = new HttpHelper().GetHtml(httpItem);
                return httpResult.Html;
            }
            static HttpResult PostDataToUrl(string url, string data, CookieCollection cookies)
            {

                //            Accept: */*
                //Accept-Language: zh-CN
                //Referer: http://9.80.35.95:8092/zhengxin_gr/index.jsp
                //x-requested-with: XMLHttpRequest
                //Content-Type: application/x-www-form-urlencoded; charset=UTF-8
                //Accept-Encoding: gzip, deflate
                //User-Agent: Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E)
                //Host: 9.80.35.95:8092
                //Content-Length: 16
                //Connection: Keep-Alive
                //Pragma: no-cache
                HttpItem httpItem = new HttpItem()
                {
                    CookieCollection = cookies,
                    URL = url,
                    Method = "POST",
                    Postdata = data,
                    //Accept="*/*",
                    //Referer = "http://9.80.35.95:8092/zhengxin_gr/index.jsp",
                    //ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    //Host = "9.80.35.95:8092",
                    //PostDataType = PostDataType.String,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                // httpItem.Header.Add("x-requested-with", "XMLHttpRequest");
                // httpItem.Header.Add("Accept-Encoding", "gzip,deflate");
                // httpItem.Header.Add("Accept-Language", "zh-CN");
                //// httpItem.Header.Add("Content-Length", "16");
                // httpItem.Header.Add("Accept-Language", "Keep-Alive");
                // httpItem.Header.Add("Pragma", "no-cache");
                HttpResult httpResult = new HttpHelper().GetHtml(httpItem);
                return httpResult;
            }
            static string read(Stream stream)
            {
                //StreamReader readStream = new StreamReader(stream, Encoding.UTF8);
                Byte[] buffer = new Byte[stream.Length];
                //从流中读取字节块并将该数据写入给定缓冲区buffer中
                stream.Read(buffer, 0, Convert.ToInt32(stream.Length));

                return System.Convert.ToBase64String(buffer);




            }
            static string GetBase64FromImage(string Imagefilename)
            {
                Bitmap bmp = new Bitmap(Imagefilename);

                //FileStream fs = new FileStream(Imagefilename, FileMode.Create);
                //StreamWriter sw = new StreamWriter(fs);

                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                String strbaser64 = Convert.ToBase64String(arr);
                //sw.Write(strbaser64);

                //sw.Close();
                //fs.Close();
                return strbaser64;
            }
            static void De()
            {
                string s = System.Web.HttpUtility.UrlDecode(System.IO.File.ReadAllText(@"D:\上海小贷\certFile.txt"), Encoding.UTF8);

                RequestXml requestMessage = null;
                using (StringReader rdr = new StringReader(s.Substring(6)))
                {
                    //声明序列化对象实例serializer
                    XmlSerializer serializer = new XmlSerializer(typeof(RequestXml));
                    //反序列化，并将反序列化结果值赋给变量i
                    requestMessage = (RequestXml)serializer.Deserialize(rdr);
                    //输出反序列化结果

                }




                FileUp fileUp = new FileUp();

                //using (StringReader rdr = new StringReader(System.IO.File.ReadAllText(@"D:\上海小贷\Log_20161205.txt", Encoding.GetEncoding("gbk"))))
                //{
                //    //声明序列化对象实例serializer
                //    XmlSerializer serializer = new XmlSerializer(typeof(RequestXml));
                //    //反序列化，并将反序列化结果值赋给变量i
                //    requestMessage = (RequestXml)serializer.Deserialize(rdr);
                //    //输出反序列化结果

                //}
                string saveDirectory = ConfigData.requestFileSavePath + DateTime.Now.ToString("yyyyMMdd");
                Vcredit.ExtTrade.CommonLayer.FileOperateHelper.FolderCreate(saveDirectory);
                fileUp.SaveFile(Convert.FromBase64String(requestMessage.MSG.QueryRequestMessage.AuthoFile),
            saveDirectory + "\\SQ.jpg");
                fileUp.SaveFile(Convert.FromBase64String(requestMessage.MSG.QueryRequestMessage.CertFile),
            saveDirectory + "\\ZJ.jpg");
            }
            private static void test()
            {
                //入库
                FileStream stream = new FileInfo(@"D:\上海小贷\jpgfile\1.jpg").OpenRead();
                FileStream stream1 = new FileInfo(@"D:\上海小贷\jpgfile\2.jpg").OpenRead();

                FileStream stream3 = new FileInfo(@"C:\Users\yejianliang\Desktop\新建文件夹\制作测试数据\ZJ_ZS_354334198603274116_20160524.jpg").OpenRead();
                FileStream stream4 = new FileInfo(@"C:\Users\yejianliang\Desktop\新建文件夹\制作测试数据\ZJ_ZJ_211381198106115133_20160316.jpg").OpenRead();

                FileStream stream5 = new FileInfo(@"C:\Users\yejianliang\Desktop\新建文件夹\制作测试数据\SQ_ZS_354334198603274116_20160524.jpg").OpenRead();
                FileStream stream6 = new FileInfo(@"C:\Users\yejianliang\Desktop\新建文件夹\制作测试数据\SQ_ZJ_211381198106115133_20160316.jpg").OpenRead();
                CreditUserInfo credit = new CreditUserInfo();
                //var user =
                //         new
                //         {

                //             cifName = "邵玲琳".ToBase64(),
                //             idType = "0",
                //             idNo = "aw221q8h07811try26",
                //             AuthorizationBase64Str = read(stream),
                //             CertBase64Str = read(stream1)

                //         };
                //var user1 =
                //      new
                //      {
                //          cifName = "23".ToBase64(),
                //          idType = "0",
                //          idNo = "1304011998010r3417",
                //          AuthorizationBase64Str = read(stream3),
                //          CertBase64Str = read(stream4)
                //      };
                //var user2 =
                //     new
                //      {

                //          cifName = "王晓丽".ToBase64(),
                //          idType = "0",
                //          idNo = "33022619g102250020",
                //          AuthorizationBase64Str = read(stream5),
                //          CertBase64Str = read(stream6)
                //      };
                var user =
                       new CRD_CD_CreditUserInfoEntity()
                       {
                           BusType = "test",
                           Name = "邵玲琳99",
                           Cert_Type = "0",
                           Cert_No = "350783198510071518",
                           //AuthorizationFileBase64String = GetBase64FromImage(@"D:\上海小贷\jpgfile\1.jpg"),// read(stream),//read(stream),
                           //CertFileBase64String = GetBase64FromImage(@"D:\上海小贷\jpgfile\2.jpg"),//read(stream1),read(stream),
                           QueryReason = "02",
                           ApplyID = 1212
                       };
                //   new ShangHaiCredit().DirectPostRequestMessage(user);
                //  string url = "http://localhost:40689/ShangHaiCredit/Add/CreditApply";
                //string url = "http://10.1.12.70:9000/ShangHaiCredit/Add/CreditApply";
                string url = "http://10.1.12.194:8002/ShangHaiCredit/Add/CreditApply";
                //   var rest = PostDataToUrl(url, JsonConvert.SerializeObject(user));
                var result = PostDataToUrl(url, JsonConvert.SerializeObject(user));

                var user1 =
                      new
                      {
                          cifName = "安东尼",
                          idType = "0",
                          idNo = "350783198510071518",
                          AuthorizationBase64Str = read(stream3),
                          CertBase64Str = read(stream4)
                      };
                var user2 =
                     new
                     {

                         cifName = "王晓丽",
                         idType = "0",
                         idNo = "33022619g10225002a",
                         AuthorizationBase64Str = read(stream5),
                         CertBase64Str = read(stream6)

                     };

                List<object> list = new List<object>();

                list.Add(user1);
                list.Add(user2);

                foreach (var item in list)
                {
                    string js = JsonConvert.SerializeObject(item);
                    string strHtml = PostDataToUrl("http://10.1.12.73:8002/OriginalExtTrade/UpLoad/AuthFile", js);
                    //string strHtml = PostDataToUrl("http://10.1.12.73:8088/ExtTrade/Upload/AuthFile", js);
                    Console.WriteLine(strHtml);
                }

            }
            //static void  test1()
            //{
            //    string va = "http://localhost:801/ExtTrade/Get/ReportIdReportSnAndState/41112219710911111";

            //    //string re = PostDataToUrl(va);
            //                BaseDao dao = new BaseDao();
            //   test();
            //  CRD_CD_CreditUserInfoBusiness sb = new CRD_CD_CreditUserInfoBusiness();

            //    var list= dao.Select<CRD_CD_CreditUserInfoEntity>("select top 10 * from credit.CRD_CD_CreditUserInfo");


            //    s.isInLimitedDays(30, "sdfsf");
            //    //Models models = new Models();
            //    //Model model = new Model();


            //    ////models.CRD_HD_REPORTList1 = dao.Select<CRD_HD_REPORTEntity>("select top 2 * from [credit].[CRD_HD_REPORT]");
            //    ////models.CRD_PI_IDENTITYList1 = dao.Select<CRD_PI_IDENTITYEntity>("select top 2 * from [credit].[CRD_PI_IDENTITY]");
            //    ////var json = JsonConvert.SerializeObject(models);
            //    ////models = JsonConvert.DeserializeObject<Models>(json);

            //    //List<Model> modelList = new List<Model>();
            //    //model.Report = dao.Select<CRD_HD_REPORTEntity>("select top 2 * from [credit].[CRD_HD_REPORT]").FirstOrDefault();
            //    //model.Identity = dao.Select<CRD_PI_IDENTITYEntity>("select top 2 * from [credit].[CRD_PI_IDENTITY]").FirstOrDefault();
            //    //model.CRD_PI_RESIDENC = dao.Select<CRD_PI_RESIDENCEEntity>("select top 3 * from [credit].CRD_PI_RESIDENCE");
            //    //model.CRD_IS_CREDITCUE = dao.Select<CRD_IS_CREDITCUEEntity>("select top 3 * from [credit].CRD_IS_CREDITCUE");
            //    //modelList.Add(model);
            //    //Model model1 = new Model();
            //    //model1.Report = dao.Select<CRD_HD_REPORTEntity>("select top 2 * from [credit].[CRD_HD_REPORT]").FirstOrDefault();
            //    //model1.Identity = dao.Select<CRD_PI_IDENTITYEntity>("select top 2 * from [credit].[CRD_PI_IDENTITY]").FirstOrDefault();
            //    //model1.CRD_PI_RESIDENC = dao.Select<CRD_PI_RESIDENCEEntity>("select top 3 * from [credit].CRD_PI_RESIDENCE");
            //    //model1.CRD_IS_CREDITCUE = dao.Select<CRD_IS_CREDITCUEEntity>("select top 3 * from [credit].CRD_IS_CREDITCUE");
            //    //modelList.Add(model1);
            //    //Message mes = new Message();
            //    //mes.CreditDataList = modelList;
            //    //List<ErrorData> errorList = new List<ErrorData>();
            //    //ErrorData er = new ErrorData()
            //    //{
            //    //    Cert_NO = "1sdfs",
            //    //    Cert_Type = "1",
            //    //    ErrorCode = "1",
            //    //    ErrorDes = "erwerw"

            //    //};
            //    //ErrorData er1 = new ErrorData()
            //    //{
            //    //    Cert_NO = "1sdfs",
            //    //    Cert_Type = "1",
            //    //    ErrorCode = "1",
            //    //    ErrorDes = "erwerw"
            //    //};
            //    //errorList.Add(er); errorList.Add(er1);
            //    //mes.ErrorDataList = errorList;
            //    //var json1 = JsonConvert.SerializeObject(mes);
            //    //var list = JsonConvert.DeserializeObject<Message>(json1);

            //    Console.Read();
            //}
            static void testhtml()
            {
                Analisis a = new Analisis();
                //DirectoryInfo dic = new DirectoryInfo(@"D:\征信报告文档\新外贸\htmltest\html文件");
                //foreach (var item in dic.GetFiles())
                //{
                //    try
                //    {
                //        a.SaveData(item.Name, File.ReadAllText(item.FullName, Encoding.GetEncoding("GBK")));
                //    }
                //    catch
                //    {

                //    }

                //}
                a.SaveData("", System.IO.File.ReadAllText(@"D:\vcData\91UU\00100101142\RecvFile\郝亮_100101153\370921198611065112.html", Encoding.GetEncoding("GBK")));
                //a.SaveData("", File.RedAllText(@"D:\征信报告文档\zx\20160721\411221199105033513.html", Encoding.GetEncoding("GBK")));
                // a.SaveData("", File.ReadAllText(@"D:\征信报告文档\zx\20160721\411221199105033513.html", Encoding.GetEncoding("GBK")));
            }
            static void testAna()
            {
                string json = System.IO.File.ReadAllText(@"D:\征信报告文档\新外贸\htmltest\信息较全.txt");
                var message = JsonConvert.DeserializeObject<NewForeignContainer>(json);
                List<NewForeignContainer> list = new List<NewForeignContainer>() { message };


                NewForeignTrade.DealwithNewForeignContainer(list);
                new ForeignComBus().SaveNewForeignCreditInfoToDB(list, "", "");//入库和修改状态

            }
            public static bool CheckIsNextToday(DateTime dtime)
            {
                bool isNext = false;
                double hourPoint = 20;

                if (double.TryParse(ConfigData.todayDealingPoint, out  hourPoint))
                {
                    if (dtime.Hour + 30 / 60.00 >= hourPoint)
                    {
                        isNext = true;
                    }
                }
                return isNext;
            }
            static void Get(ref decimal age, ref string name)
            {
                age = 10;
                name = "sfd";
            }

            private static void GetEmptyJson()
            {
                string jsong = "[]";
                var list = JsonConvert.DeserializeObject<List<person>>(jsong);
            }
            private static void Mapping()
            {
                string json = "{\"state\":\"\",\"State_End_Date\":null}";
                var en = JsonConvert.DeserializeObject<CRD_CD_LNEntity>(json);
            }
            private static void test4()
            {
                try
                {

                    SingleRequest sreq = new SingleRequest();
                    SingleCreditReq scredireq = new SingleCreditReq()
                    {
                        cifName = "张慧".ToBase64(),
                        idNo = "310113198810125312",
                        idType = "0",
                        czPactNo = "sdf",
                        czAuth = "1",
                        czId = "1",
                    };
                }
                catch (Exception ex)
                {

                    Log4netAdapter.WriteError("", ex);
                }

            }
            static List<string> list = new List<string>();
            private static void testq()
            {
                BaseDao dao = new BaseDao();

                List<CRD_HD_REPORTEntity> creidtlist = new List<CRD_HD_REPORTEntity>();
                // creidtlist.Add(new CRD_HD_REPORTEntity() { Name = "徐昌顺", Cert_No = "340521199409241052" });
                //  creidtlist = dao.Select<CRD_HD_REPORTEntity>(x => x.Report_Id >= 1346 && x.Report_Id <= 2513);
                // creidtlist = dao.Select<CRD_HD_REPORTEntity>(x => x.Report_Id == 5279);

                //  creidtlist.Add(new CRD_HD_REPORTEntity() { Name = "温原", Cert_No = "120102198207233218" });
                List<SingleCreditReq> slist = new List<SingleCreditReq>();
                Dictionary<string, string> dic = new Dictionary<string, string>(){
            {"130401199801013417","23"},
            };
                foreach (var item in dic)
                {
                    slist.Add(new SingleCreditReq() { cifName = item.Value.ToBase64(), idNo = item.Key, czId = "1", czAuth = "1", idType = "0", BusType = "test" });
                }

                //SingleCreditReq scredireq = new SingleCreditReq()
                //{
                //    cifName = "温原", 
                //    idNo = "120102198207233218",
                //    idType = "0",
                //    czAuth ="1",
                //    czId = "1",
                //};
                //SingleCreditReq scredireq1 = new SingleCreditReq()
                //{
                //    cifName = "叶明功",
                //    idNo = "34012219791010003X",
                //    idType = "0",
                //    czAuth = true,
                //    czId = true,
                //};
                Func<CRD_HD_REPORTEntity, SingleCreditReq> fun = (para) =>
                    {
                        return new SingleCreditReq()
                 {
                     cifName = para.Name.ToBase64(),
                     idNo = para.Cert_No,
                     idType = "0",
                     czAuth = "1",
                     czId = "1",
                     ApplyID = 174
                 };
                    };
                // {"cifName":"促销","idNo":"350783198510071518 ","idType":"0","czAuth":"1","ApplyID":0,"czId":"1","BusType":"DOUDOUQIAN","crpReason":"01"}
                var user =
                         new
                         {

                             cifName = "邹丽红",
                             idType = "0",
                             idNo = "370204195707252328",
                             czAuth = "1",
                             czId = "1",
                             BusType = "VBS",
                             crpReason = "02",
                             ApplyID = 14336759,

                         };

                var user1 =
                      new
                      {

                          cifName = "王子连",
                          idType = "0",
                          idNo = "330203195609141516",
                          czAuth = "1",
                          czId = "1",
                          BusType = "VBS",
                          crpReason = "02",
                          ApplyID = 14320344,

                      };



                var user2 =
                     new
                     {

                         cifName = "何凤民",
                         idType = "0",
                         idNo = "330203195803021526",
                         czAuth = "1",
                         czId = "1",
                         BusType = "VBS",
                         crpReason = "02",
                         ApplyID = 14320343,

                     };

                //var user3 =
                //   new
                //   {

                //       cifName = "刘建华",
                //       idType = "0",
                //       idNo = "360102195603165342",
                //       czAuth = "1",
                //       czId = "1",
                //       BusType = "test",
                //       crpReason = "02",
                //       ApplyID = 14266631

                //   };

                var user4 =
                      new
                      {
                          cifName = "肖兆夫",
                          idType = "0",
                          idNo = "320521195911123316",
                          czAuth = "1",
                          czId = "1",
                          BusType = "test",
                          crpReason = "02",
                          ApplyID = 182
                      };




                var user5 =
                     new
                     {
                         cifName = "陈家辉",
                         idType = "0",
                         idNo = "330329197804201738",
                         czAuth = "1",
                         czId = "1",
                         BusType = "test",
                         crpReason = "02",
                         ApplyID = 183
                     };
                var user6 =
             new
             {
                 cifName = "徐力",
                 idType = "0",
                 idNo = "320582198806260817",
                 czAuth = "1",
                 czId = "1",
                 BusType = "test",
                 crpReason = "02",
                 ApplyID = 184
             };

                var user7 =
                   new
                   {

                       cifName = "凌晨辉",
                       idType = "0",
                       idNo = "320223197106137075",
                       czAuth = "1",
                       czId = "1",
                       BusType = "test",
                       crpReason = "02",
                       ApplyID = 185

                   };

                var user8 =
                      new
                      {
                          cifName = "王国兴",
                          idType = "0",
                          idNo = "320911199205105310",
                          czAuth = "1",
                          czId = "1",
                          BusType = "test",
                          crpReason = "02",
                          ApplyID = 187
                      };




                var user9 =
                     new
                     {
                         cifName = "丰世权",
                         idType = "0",
                         idNo = "330721197802225518",
                         czAuth = "1",
                         czId = "1",
                         BusType = "test",
                         crpReason = "02",
                         ApplyID = 188
                     };

                var user10 =
                     new
                     {
                         cifName = "姚坚",
                         idType = "0",
                         idNo = "330122198505062810",
                         czAuth = "1",
                         czId = "1",
                         BusType = "test",
                         crpReason = "02",
                         ApplyID = 190
                     };

                list.Add(JsonConvert.SerializeObject(user));
                list.Add(JsonConvert.SerializeObject(user1));
                list.Add(JsonConvert.SerializeObject(user2));
                //list.Add(JsonConvert.SerializeObject(user3));
                //list.Add(JsonConvert.SerializeObject(user4));

                //list.Add(JsonConvert.SerializeObject(user5));
                //list.Add(JsonConvert.SerializeObject(user6));
                //list.Add(JsonConvert.SerializeObject(user7));
                //list.Add(JsonConvert.SerializeObject(user8));
                //list.Add(JsonConvert.SerializeObject(user9));
                //list.Add(JsonConvert.SerializeObject(user10));

                //string addurl = "http://10.1.12.194:8002/ExtTrade/Add/CreditInfo";
                string addurl = "http://localhost:40689/ExtTrade/Add/CreditInfo";
                // string geturl = "http://10.1.12.70:7000/ExtTrade/Get/CreditInfo";
                //string geturl = "http://10.1.12.73:8088/ExtTrade/Get/CreditInfo";
                // string url = "http://10.1.12.73:8088/OriginalExtTrad/Upload/AuthFile";
                // var getresult = PostDataToUrl(geturl, info); 
                int index = 0;
                foreach (var item in list)
                {

                    //SingleRequest.ReceiveCreditInfo(JsonConvert.DeserializeObject<SingleCreditReq>(item));
                    var addresult = PostDataToUrl(addurl, item);
                    Console.WriteLine("第" + index + "个征信报告：" + addresult);

                    index++;
                }

                Console.Read();

            }
            static void test1()
            {

            }

            static void testNewFore()
            {
                DataTable dt = NPOIHelper.GetDataTable(@"D:\征信报告文档\新外贸\外贸新接口测试名单（贷后管理）.xlsx", true);
                string addurl = "http://10.1.12.73:8002/ExtTrade/Add/CreditInfo";
                List<string> list = new List<string>();
                foreach (DataRow dr in dt.Rows)
                {
                    var user9 =
                    new
                    {
                        cifName = dr[1].ToString().ToBase64(),
                        idType = "0",
                        idNo = dr[2].ToString(),
                        czAuth = "1",
                        czId = "1",
                        BusType = "test",
                        crpReason = "01"
                    };
                    list.Add(JsonConvert.SerializeObject(user9));
                }
                foreach (var item in list)
                {
                    var addresult = PostDataToUrl(addurl, item);
                }
            }
            static void additem(object item)
            {
                var addresult = PostDataToUrl("http://10.1.12.73:8002/ExtTrade/Add/CreditInfo", item.ToString());
                Console.WriteLine("征信报告：" + addresult);
            }
            static void testquery()
            {
                //  new CRD_CD_CreditUserInfoBusiness().GetNotUploadFileInfo();  "

                string addurl = "http://10.1.12.70:8082/ExtTrade/Update/ApplyID";
                //   string addurl = "http://localhost:40689/ExtTrade/Update/ApplyID";
                //   string addurl = "http://localhost:40689/OrgCredit/Query/CreditInfo";
                // string addurl = "http://10.1.12.73:8002/OrgCredit/Query/CreditInfo";
                // string addurl = "http://localhost:40689/Credit/Query/BusTypeEnum";
                // string addurl = "http://10.1.12.73:8002/OrgCredit/Query/BusTypeEnum";

                // string addurl = "http://10.1.12.194:8002/OrgCredit/Query/ReportInfo";
                //string addurl = "http://localhost:40689/OrgCredit/Query/ReportInfo";
                //   string addurl = "http://10.1.12.194:8002/OrgCredit/Query/StateInfo";
                //  string addurl = "http://localhost:40689/OrgCredit/Query/IdentityInfo";
                //    string addurl = "http://localhost:40689/OrgCredit/Query/RequestRecord";
                var req = new { Cert_No = "410927196110125019", Name = "张开芳", Date = "20171215", ApplyID = 12219351, BusType = "VBS" };

                //   var req = new { ReportSn = "2016071900003024784014",ReportId= 14};
                var result = PostDataToUrl(addurl, JsonConvert.SerializeObject(req));
                Console.WriteLine(result);

            }
            static void compare()
            {
                BaseDao dao = new BaseDao();
                var result = dao.Select<CRD_HD_REPORTEntity>(x => x.Report_Id >= 1341);
                var group = result.GroupBy(x => x.Report_Sn);
                var g = group.Where(x => x.Count() > 1);
                foreach (var item in g)
                {
                    if (item.Count() == 1)
                        continue;
                    var en1 = item.Where(x => x.SourceType == 10).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                    if (en1 == null)
                        continue;
                    var en2 = item.Where(x => x.SourceType == 11).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                    if (en2 == null)
                        continue;
                    var lndList = dao.Select<CRD_CD_LNEntity>(x => x.Report_Id == en1.Report_Id);
                    var lndList1 = dao.Select<CRD_CD_LNEntity>(x => x.Report_Id == en2.Report_Id);
                    if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                    {
                        Console.WriteLine("report_id:" + en1.Report_Id + "与reportid1" + en2.Report_Id + "存储的数量不一致");
                        continue;
                    }
                    if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                    {
                        lndList = lndList.OrderBy(x => x.Cue).ToList();
                        lndList1 = lndList1.OrderBy(x => x.Cue).ToList();
                        for (int i = 0; i < lndList1.Count; i++)
                        {
                            comparetoenttiy(lndList[i], lndList1[i]);
                        }
                    }
                    else
                    {
                        Console.WriteLine("report_id:" + en1.Report_Id + "与reportid1" + en2.Report_Id + "存储实例的数量不一致");
                    }
                }

                Console.Read();
            }
            static void comparetoenttiy<T>(T entity1, T entity2)
            {
                var pros = entity1.GetType().GetProperties();
                var pros1 = entity2.GetType().GetProperties();
                object reportid = entity1.GetPropertyValue("Report_Id");
                object reportid1 = entity2.GetPropertyValue("Report_Id");
                for (int i = 0; i < pros.Length; i++)
                {
                    if (pros[i].Name.Contains("_Id") || pros[i].Name == "Time_Stamp" || pros[i].Name == "TIMESTAMP" || pros[i].Name == "CarDType" || pros[i].Name == "SettlementDate" || pros[i].Name == "CancellationDate")
                        continue;
                    if (pros[i].Name == pros1[i].Name)
                    {
                        if ((pros[i].GetValue(entity1) ?? string.Empty).ToString() == (pros[i].GetValue(entity2) ?? string.Empty).ToString())
                        {

                        }
                        else
                        {

                            Console.WriteLine(reportid.ToString() + "属性" + pros[i].Name + "的值" + pros[i].GetValue(entity1) + ",不等于" + reportid1.ToString() + "属性" + pros1[i].Name + "的值" + pros1[i].GetValue(entity2));
                        }
                    }
                }
            }
            static object GetGuarantee_Type(string str, string endUnitStr)
            {
                int index = str.IndexOf(endUnitStr);
                if (index == -1)
                {
                    if (endUnitStr == "担保")
                    {
                        index = str.IndexOf("保证");
                        if (index == -1)
                        {
                            index = str.IndexOf("保");
                            if (index == -1)
                                return DBNull.Value;
                            else
                                endUnitStr = "保";
                        }
                    }
                    else
                    {
                        return DBNull.Value;
                    }

                }
                int lastindex = str.Substring(0, index).LastIndexOf("，");
                return str.Substring(lastindex + 1, index - lastindex + endUnitStr.Length - 1);
            }

            static object GetOpenDate(string str)
            {
                DateTime dt = DateTime.Now;
                var opendate = str.Substring(str.IndexOf('.') + 1, 11).Replace("年", "-").Replace("月", "-").Replace("日", "");
                if (DateTime.TryParse(opendate, out dt))
                    return opendate;
                return DBNull.Value;


            }
            static void objexttocxml()
            {
                UpLoadXmlFile Root = new UpLoadXmlFile();
                Root.Task = new Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade.Work();
                Root.Task.SequenceId = "sdfsdf";
                Root.Task.BranchCode = "sdfsf";
                Root.Task.Source = "sfds";
                Root.Task.OtherParam = "sdfs";
                List<Document> documents = new List<Document>();
                documents.Add(new Document()
                {
                    Busstype = "sf",
                    Subtype = "sdf",
                    FilesCount = 2,
                    Files = new List<Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade.File>() { 
              new Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade.File(){FileOrder=1,Name="sdf"},
              new Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade.File(){FileOrder=2,Name="sdf"},
              },
                });

                Root.Task.Documents = documents;

                XmlSerializer serializer = new XmlSerializer(Root.GetType());
                TextWriter writer = new StreamWriter(@"D:/Department.xml");
                serializer.Serialize(writer, Root);
                writer.Close();
                FileStream firle = new FileStream("D:/Department.xml", FileMode.Open);
                var re = (UpLoadXmlFile)serializer.Deserialize(firle);


                firle.Close();

            }
            static void CheckQueryRepeat()
            {
                CRD_CD_CreditUserInfoBusiness creidt = new CRD_CD_CreditUserInfoBusiness();
                string[] array = System.IO.File.ReadAllLines(@"D:\外贸查询重复记录.txt", Encoding.GetEncoding("GBK"));
                List<checkInfo> chList = new List<checkInfo>();
                List<checkInfo> chListCount = new List<checkInfo>();
                foreach (var item in array)
                {
                    var res = item.Split(',');
                    chList.Add(new checkInfo()
                    {
                        Name = res[0],
                        CertNO = res[1].Replace(" ", ""),
                        checkTime = res[2].Insert(4, ".").Insert(7, ".").ToDateTime().Value
                    });
                }
                var chGroup = chList.GroupBy(x => x.CertNO);
                StringBuilder sb = new StringBuilder();
                int totalCount = 0;
                foreach (var item in chGroup)
                {
                    var itemre = item.OrderBy(x => x.checkTime).ToList();
                    chListCount.Add(itemre.First());
                    totalCount += 1;
                    if (item.Count() == 1)
                    {
                        continue;
                    }
                    DateTime dt = itemre[0].checkTime;
                    for (int i = 1; i < itemre.Count; i++)
                    {
                        var dt1 = itemre[i].checkTime;
                        if (dt.AddDays(30).CompareTo(dt1) < 0)
                        {
                            totalCount += 1;
                            dt = dt1;
                            chListCount.Add(itemre[i]);
                        }

                    }

                }
                var chListcR = chListCount.OrderBy(x => x.CertNO);
                foreach (var item in chListcR)
                {
                    sb.AppendLine(item.ToString());
                }
                Vcredit.ExtTrade.CommonLayer.FileOperateHelper.WriteFile(@"D:\征信报告文档\新外贸\征信查询情况\机构版征信查询情况.txt", sb.ToString());
            }
            static void CheckQuery()
            {
                CRD_CD_CreditUserInfoBusiness creidt = new CRD_CD_CreditUserInfoBusiness();
                string[] array = System.IO.File.ReadAllLines(@"D:\外贸查询重复记录.txt", Encoding.GetEncoding("GBK"));
                StringBuilder sb = new StringBuilder();
                for (int i = 1; i < array.Length; i++)
                {
                    var list = creidt.GetAllListByCert_no(array[i].Split(',')[5]);
                    sb.Append(array[i] + ",");
                    if (list.Count == 0)
                    {
                        sb.Append("没有查询记录");
                    }
                    else if (list.Count == 1)
                    {
                        if (list[0].SourceType == 10)
                        {
                            sb.Append("于" + list[0].Time_Stamp + "在外贸查询");
                        }
                        else
                        {
                            sb.Append("于" + list[0].Time_Stamp + "在担保查询");
                        }
                    }
                    else
                    {
                        foreach (var item in list.Where(x => x.SourceType == 10))
                        {
                            sb.Append("于" + item.Time_Stamp + "在外贸查询--");
                        }
                        foreach (var item in list.Where(x => x.SourceType == 11))
                        {
                            sb.Append("于" + item.Time_Stamp + "在担保查询--");
                        }
                    }
                    sb.Append("\r\n");

                }
                Vcredit.ExtTrade.CommonLayer.FileOperateHelper.WriteFile(@"D:\征信报告文档\新外贸\征信查询情况\机构版征信查询情况.txt", sb.ToString());
            }

            static void CheckExtTradeQuery()
            {
                CRD_CD_CreditUserInfoBusiness creidt = new CRD_CD_CreditUserInfoBusiness();
                DataTable dt = NPOIHelper.GetDataTable(@"D:\新建文件夹\2016维信查征明细.xlsx", true);
                IEnumerable<IGrouping<string, DataRow>> result = dt.Rows.Cast<DataRow>().GroupBy<DataRow, string>(dr => (dr[2].ToString() + dr[3].ToString()).Replace(" ", ""));

                var recount = result.Count();

                var results = result.Where(x => x.Count() > 1);

                var counts = results.Count();
                StringBuilder sb1 = new StringBuilder();
                int index = 0;
                foreach (var item in results)
                {
                    foreach (var item1 in item)
                    {
                        sb1.AppendLine(item1[1].ToString() + "," + item1[2].ToString() + "," + item1[3].ToString());
                        index++;
                    }
                }

                StringBuilder sb = new StringBuilder();
                Console.WriteLine("总共" + dt.Rows.Count);
                BaseDao bd = new BaseDao();
                var allList = bd.Select<CRD_CD_CreditUserInfoEntity>();


                Parallel.For(0, dt.Rows.Count, i =>
                    {
                        List<string> listHave = new List<string>();
                        var list = allList.Where(x => x.Cert_No == dt.Rows[i][2].ToString()).ToList(); //creidt.GetAllListByCert_no(dt.Rows[i][2].ToString());
                        if (list.Count == 0)
                        {
                            sb.AppendLine(dt.Rows[i][1].ToString() + "," + dt.Rows[i][2] + ",没有查询记录");
                        }
                        else if (list.Count == 1)
                        {
                            if (list[0].SourceType == 10)
                            {
                                if (list[0].State != 5 && list[0].State != 7)
                                {
                                    sb.AppendLine(dt.Rows[i][1].ToString() + "," + dt.Rows[i][2] + ",在外贸查询的状态是" + GetStateDesc(list[0].State.ToString()));
                                }

                            }
                            else
                            {
                                sb.AppendLine(dt.Rows[i][1].ToString() + "," + dt.Rows[i][2] + ",在担保查询");

                            }
                        }
                        else
                        {

                            foreach (var item in list.Where(x => x.SourceType == 10))
                            {
                                if (item.State != 5 && item.State != 7)
                                {
                                    sb.AppendLine(dt.Rows[i][1].ToString() + "," + dt.Rows[i][2] + ",在外贸查询的状态是" + GetStateDesc(list[0].State.ToString()));

                                }
                                else
                                {
                                    listHave.Add(item.Cert_No);
                                }

                            }
                            foreach (var item in list.Where(x => x.SourceType == 11))
                            {
                                if (!listHave.Contains(item.Cert_No))
                                    sb.AppendLine(dt.Rows[i][1].ToString() + "," + dt.Rows[i][2] + ",在担保查询");

                            }
                        }
                        Console.WriteLine("第" + (i + 1).ToString() + "条，还有" + (dt.Rows.Count - i - 1).ToString() + "条");

                    });
                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //    var list = allList.Where(x => x.Cert_No == dt.Rows[i][2].ToString()).ToList(); //creidt.GetAllListByCert_no(dt.Rows[i][2].ToString());
                //    if (list.Count == 0)
                //    {
                //        sb.AppendLine(dt.Rows[i][1].ToString()+","+dt.Rows[i][2]+",没有查询记录");
                //    }
                //    else if (list.Count == 1)
                //    {
                //        if (list[0].SourceType == 10)
                //        {
                //            if(list[0].State!=5&&list[0].State!=7)
                //            {
                //                sb.AppendLine(dt.Rows[i][1].ToString() + "," + dt.Rows[i][2] + ",在外贸查询的状态是" + GetStateDesc(list[0].State.ToString()));
                //            }

                //        }
                //        else
                //        {
                //            sb.AppendLine(dt.Rows[i][1].ToString() + "," + dt.Rows[i][2] + ",在担保查询");

                //        }
                //    }
                //    else
                //    {

                //        foreach (var item in list.Where(x => x.SourceType == 10))
                //        {
                //            if (item.State != 5 && item.State != 7)
                //            {
                //                sb.AppendLine(dt.Rows[i][1].ToString() + "," + dt.Rows[i][2] + ",在外贸查询的状态是" + GetStateDesc(list[0].State.ToString()));

                //            }
                //            else
                //            {
                //                listHave.Add(item.Cert_No);
                //            }

                //        }
                //        foreach (var item in list.Where(x => x.SourceType == 11))
                //        {
                //            if (!listHave.Contains(item.Cert_No))
                //            sb.AppendLine(dt.Rows[i][1].ToString() + "," + dt.Rows[i][2] + ",在担保查询");

                //        }
                //    }
                //    listHave.Clear();
                //    Console.WriteLine("第"+(i+1).ToString()+"条，还有"+(dt.Rows.Count-i-1).ToString()+"条");

                //}
                Vcredit.ExtTrade.CommonLayer.FileOperateHelper.WriteFile(@"D:\征信报告文档\新外贸\征信查询情况\机构版征信查询情况.txt", sb.ToString());
            }
            private static string GetStateDesc(string state)
            {
                string statedes = string.Empty;
                switch (state)
                {
                    case "0":
                        statedes = "初始化";
                        break;
                    case "1":
                        statedes = "上传失败";
                        break;
                    case "2":
                        statedes = "上传成功";
                        break;
                    case "3":
                        statedes = "查询失败";
                        break;
                    case "4":
                        statedes = "解析失败";
                        break;
                    case "5":
                        statedes = "成功获取";
                        break;
                    case "6":
                        statedes = "正在上传";
                        break;
                    case "7":
                        statedes = "征信空白";
                        break;

                }
                return statedes;
            }
            private static void Getsumrecord()
            {
                ForeignComBus fobus = new ForeignComBus();
                BaseDao dao = new BaseDao();
                var reportlist = dao.Select<CRD_HD_REPORTEntity>("SELECT Report_Id,Report_Create_Time,Cert_No FROM credit.CRD_HD_REPORT WHERE SourceType=10 AND Time_Stamp>'2016-10-1' AND Report_Id NOT IN(SELECT Report_Id FROM credit.CRD_QR_RECORDDTL)");

                int i = 1;

                using (var db = dao.Open())
                {
                    foreach (var item in reportlist)
                    {
                        var recordList = db.Select<CRD_QR_RECORDDTLINFOEntity>(x => x.Report_Id == item.Report_Id);
                        fobus.InsertCRD_QR_RECORDDTLEntity(db, recordList, item.Report_Create_Time, item.Report_Id);
                        Console.WriteLine(i);
                        i++;
                    }
                }
            }
            private static void Testzx()
            {
                //   ShangHaiCredit shang = new ShangHaiCredit();
                //   shang.xml = new Vcredit.ExtTrade.CommonLayer.XmlHelper(System.IO.File.ReadAllText(@"D:\征信报告文档\上海小贷\330723198505014121.xml", Encoding.UTF8));
                //   var list=  new CRD_CD_CreditUserInfoBusiness().GetByCertNo("350681199102180527", "甘晓燕", "0");
                ////   shang.GetJinXmlCreditInfo(list[0]);
            }
            private static void Testqe()
            {
                //   ShangHaiCredit shang = new ShangHaiCredit();
                //   shang.xml = new Vcredit.ExtTrade.CommonLayer.XmlHelper(System.IO.File.ReadAllText(@"D:\征信报告文档\上海小贷\8000或8001-响应.xml", Encoding.UTF8));
                //   var list = new CRD_CD_CreditUserInfoBusiness().GetByCertNo("350681199102180527", "甘晓燕", "0");
                ////   shang.GetJinXmlCreditInfo(list[0]);
            }

            private void convettostring()
            {
                //         FileUp fileUp = new FileUp(); 
                //         fileUp.SaveFile(Convert.FromBase64String(requestMessage.AuthorizationFileBase64String),
                //saveDirectory + "\\SQ_" + requestMessage.Cert_No + "_" + dateStr + ".jpg");

            }

            private static void TestAssure()
            {
                string data = "{\"CertType\":\"0\",\"CertNo\":\"411403198703138431\",\"QueryReason\":\"03\",\"Name\":\"姜向威\",\"BusType\":\"ANJIAPAI\",\"ApplyID\":153}";
                // PostDataToUrl("http://10.1.12.39:8012/api/Assure/AddQueryInfo", data);
                PostDataToUrl("http://localhost:5200/api/Assure/AddQueryInfo", data);
            }
            static void TestGetFiles()
            {
                var result = PostDataToUrl("http://10.138.60.115:8099/Compliance/GetUserFiles", "{\"ApplyID\":174,\"Name\":\"测试\",\"IdentityNo\":\"411522199103030081\"}");
                var reponet = JsonConvert.DeserializeObject<UserFilesRespone>(result);
                var files = JsonConvert.DeserializeObject<UserFileContent>(reponet.Content);
                FileUp fileup = new FileUp();
                fileup.SaveFile(Convert.FromBase64String(files.IdCardImg), @"D:\232.jpg");
                fileup.SaveFile(Convert.FromBase64String(files.InfoPowerSignatured), @"D:\sfsdf." + files.InfoPowerSignaturedFileType);
            }

            public static dynamic ChangeTo(dynamic source, Type dest)
            {
                return System.Convert.ChangeType(source, dest);
            }
            public static void ChengDuLogin()
            {
                CookieCollection cookies = new CookieCollection();


                var result = PostDataToUrlGet("http://9.80.35.95:8092/zhengxin_gr/logon.jsp");
                cookies = CommonFun.GetCookieCollection(cookies, result.CookieCollection);
                foreach (var item in cookies)
                    Console.WriteLine("cookie:" + item.AsString());
                result = PostDataToUrlGetByCookie("http://9.80.35.95:8092/zhengxin_gr/logon.do?method=login&name=cdws01&pws=cdwslutao8899&time=" + DateTime.Now, cookies);

                Console.WriteLine(result.Html);
                Log4netAdapter.WriteInfo(result.Html);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    cookies = CommonFun.GetCookieCollection(cookies, result.CookieCollection);
                    //result = PostDataToUrlGetByCookie("http://9.80.35.95:8092/zhengxin_gr/index.jsp", cookies);
                    //Console.WriteLine("默认页面"+result.Html);
                    //cookies = CommonFun.GetCookieCollection(cookies, result.CookieCollection);
                    //result = PostDataToUrlGetByCookie("http://9.80.35.95:8092/zhengxin_gr/menu.do?method=menujs&_dc=1490952206281", cookies);

                    //Console.WriteLine("获取菜单"+result.Html);
                    //cookies = CommonFun.GetCookieCollection(cookies, result.CookieCollection);

                    //result = PostDataToUrlGetByCookie("http://9.80.35.95:8092/zhengxin_gr/logon.do?method=passwordIsExpired&random=0.34325232450151993", cookies);
                    // cookies = CommonFun.GetCookieCollection(cookies, result.CookieCollection);
                    // Console.WriteLine("PasswordIsOut:"+result.Html);
                    //  foreach (var item in cookies)
                    //      Console.WriteLine(item.AsString());
                    result = PostDataToUrlGetByCookie(@"http://9.80.35.95:8092/zhengxin_gr/RequestDispathcherAtcion.do1?method=translate&dpsubsys=cu&dpurl=queryAction.do&username=" + "叶建梁".ToUrlEncode() + "&certype=0&cercode=350783198510071518&queryreason=02&vertype=30&idauthflag=0", cookies);
                    result = PostDataToUrl("http://9.80.35.95:8092/zhengxin_gr/QpCreditReport.do?method=initQuery&random=0.3221627544790892", "start=0&limit=15", cookies);
                    Console.WriteLine("Result:" + result.Html);
                    Log4netAdapter.WriteInfo("Respone:" + result.Html);
                }

            }
            static object obj = new object();
            static BaseDao dao = new BaseDao();
            private static void Dealingxml()
            {
                UpLoadXmlFile Root = new UpLoadXmlFile();
                StreamReader read = null;
                StreamWriter writer = null;
                DirectoryInfo info = new DirectoryInfo(@"D:\征信报告文档\新外贸\描述文件\描述文件"); ;
                //     var lis = dao.Select<CRD_CD_CreditUserInfoEntity>((x => x.SourceType ==10&&x.FileState ==4));
                try
                {
                    var fiels = info.GetFiles();
                    foreach (var item in fiels)
                    {
                        XmlSerializer serializer = new XmlSerializer(Root.GetType());
                        read = new StreamReader(item.FullName);
                        var xmlentity = (UpLoadXmlFile)serializer.Deserialize(read);
                        IEnumerable<CRD_CD_CreditUserInfoEntity> ress = null;
                        foreach (var doc in xmlentity.Task.Documents)
                        {
                            if (string.IsNullOrEmpty(doc.BussNo))
                            {
                                var array = doc.Files[0].Name.Split('_');
                                var cert_no = array[1];
                                var date = array[2].Substring(0, 8);
                                ress = dao.Select<CRD_CD_CreditUserInfoEntity>(x => x.SourceType == 10 && x.Cert_No == cert_no).OrderByDescending(x => x.CreditUserInfo_Id);
                                var count = ress.Count();
                                if (count == 0)
                                    throw new Exception("yic");
                                if (count > 1)
                                {
                                    var res = ress.Where(x => x.Authorization_Date == date);
                                    if (res.Count() != 0)
                                        ress = res;
                                }

                                if (string.IsNullOrEmpty(ress.First().PactNo))
                                    throw new Exception("yic");
                                doc.BussNo = ress.First().PactNo;
                            }
                        }
                        writer = new StreamWriter(@"D:\征信报告文档\新外贸\描述文件\xml\" + item.Name);
                        serializer.Serialize(writer, xmlentity);

                        if (read != null)
                            read.Close();
                        if (writer != null)
                            writer.Close();
                    }


                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError("", ex);
                    return;
                }
                finally
                {
                    if (read != null)
                        read.Close();
                    if (writer != null)
                        writer.Close();
                }
            }
            static void InsertCerditInfo()
            {

                CRD_CD_CreditUserInfoBusiness credit = new CRD_CD_CreditUserInfoBusiness();
                BaseDao dao1 = new BaseDao("ExternalTradeDB1");
                var list = dao1.Select<CRD_CD_CreditUserInfoEntity>(@"select top 100 * from  [ExtCredit].[credit].[CRD_CD_CreditUserInfo]
           
          where state =5 and sourcetype =10  order by credituserinfo_id desc ");



                BaseDao dao = new BaseDao("ExternalTradeDB");
                foreach (var item in list)
                {
                    dao.Insert(item);
                    var report = dao1.Select<CRD_HD_REPORTEntity>(x => x.Report_Sn == item.Report_sn).FirstOrDefault();
                    if (report != null)
                        dao.Insert(report);

                }
            }

            static void testdb()
            {
                BaseDao dao = new BaseDao();
                var reportlist = dao.Select<Call_LogEntity>(@"select [Id]
         ,[ModuleName]
         ,[IdentityNo]
         ,[CustName]
         ,[CallFrom],CreateTime from 
(
    select[Id]
         ,[ModuleName]
         ,[IdentityNo]
         ,[CustName]
         ,[CallFrom]
         ,[CreateTime],row_number() over  
    (
        order by  
        [Id] desc
    ) rowNum 
    from [RC_Log].[dbo].CALL_LOG_201711 WITH(NOLOCK) where createtime between '2017-11-08 00:00:00' and '2017-11-09 00:00:00'
) Temp 
where Temp.rowNum >=1 
and Temp.rowNum <= 100");

                int i = 1;

                using (var db = dao.Open())
                {

                }

            }
            static void Main(string[] args)
            {

             //   testdb();

                ////  var res= JsonConvert.SerializeObject(new OrgCreditContainer());
                // var s = MapperHelper.Map<CRD_HD_REPORTEntity, CRD_HD_REPORTDto>(null);
                //   Console.WriteLine();
                //   new ShangHaiCredit().PostReqMeg();
                //Dealingxml();
                //var a= Commonfunction.GetRandomizer(6, true, false, true, true);
                //Console.WriteLine(a);
                // ChengDuLogin();
                //var s= PostDataToUrlGet("http://10.1.12.73:8002/ExtTrade/Get/ReportState/610322198311284878");
                // PostDataToUrl("http://10.1.12.73:8002/ExtTrade/Get/ReportState")
                //var result=  new ComplianceServiceImpl().IsSignatured(new Vcredit.ExternalCredit.Services.Requests.IsSignaturedRequest()
                // {
                //     ApplyID = 1344,
                //     Name = "谷三水",
                //     IdentityNo = "522624198810306786"

                // });
                //TestGetFiles();
                // TestAssure();
                // CheckQueryRepeat();
                // CheckExtTradeQuery();
                //TestExtTradeData.checkfromdiffdb();
                //TestExtTradeData.checktestdbfromdifftype();
                //Console.Read();
                // new TestCompliance().TestApply();
                //   testNewFore();
                // De();
                //Testqe();
                // Testzx();
                //  var str = System.IO.File.ReadAllText(@"D:\征信报告文档\新外贸\新外贸字典表\金玉刚210303197210302039(1).txt", Encoding.UTF8);
                //CheckQuery();

                //objexttocxml();
                // CreditSummaryTest.GetCreditSummary();
                //testquery();
                // testAna();
                //  test();
                // var re= GetOpenDate("1.2009年12月12日商业银行“AZ”发放的42,000元（人民币）其他贷款，业务号X，抵押担保，006期，按月归还。截至2010年03月30日，账户状态为“结清”。");
                //   var s = JsonConvert.DeserializeObject<Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade.Message>( System.IO.File.ReadAllText(@"D:\征信报告文档\json.txt", Encoding.GetEncoding("GBK")));

                //var list = JsonConvert.DeserializeObject<List<Vcredit.ExtTrade.ModelLayer.CRD_CD_CreditUserInfoEntity>>(File.ReadAllText(@"D:\征信报告文档\新外贸\htmltest\test.txt", Encoding.GetEncoding("GBK")));
                // var re=  new CRD_CD_CreditUserInfoBusiness().Select("","","","");

                // compare();
              // testhtml();
              // testq();
               var batno = "0011502935104861";
               ForeignComBus foreignComBus = new ForeignComBus();
               var message = NewForeignTrade.SavetoDb(foreignComBus, System.IO.File.ReadAllText(@"D:\征信报告文档\json.txt", Encoding.GetEncoding("GBK")), batno, "001");
               List<string> errorList = null;
               if (message.Item1.ErrorDataList != null)
                   errorList = message.Item1.ErrorDataList.Select(x => x.Cert_NO).ToList();

               CreditInfoPush.current.PushExtCredit(message.Item2, errorList, batno);//推送数据
                //testq();
                // NewForeignCommon forign = new NewForeignCommon();
                //RequestEntity resq=new RequestEntity (){
                //txCode=1000,
                //token="sfd",
                //reqTime="sfd", 
                //reqDate=3434,
                //content ="sfd",
                //reqSerial="sfds"

                //};
                //var req= new RequestEntity();
                //forign.AssignmentForReqest(resq, req);
                // test4();
                //var s = PostDataToUrl("http://10.7.101.39:9001/mfs/services/searchService?wsdl", "");
                //  testhtml();
                // var ln = new ReportCaculation().GetByReportid(32);
                //  Mapping();
                // GetEmptyJson();
                //  new NewForeignTrade().GetForeignCredit();
                // string json = "{\"age\":\"358sf675\",\"name\":\"yejianliang\"}";
                //  //json = JsonConvert.SerializeObject(new person() { age=1,name="yejianliang"});
                //JsonSerializerSettings jsSettings = new JsonSerializerSettings();

                //jsSettings.Converters.Add(new BoolConvert());

                //person persons = JsonConvert.DeserializeObject<person>(json);
                //  json = "{age:\"358,675\",name=\"yejian\"}";
                //  persons = JsonConvert.DeserializeObject<person>(json);
                //new NewForeignTrade().GetForeignCredit();
                // person p = new person();
                // Get(ref  p.age, ref p.name);
                // Console.Read();
                //string patter= "[1-7]";
                //string input = "sdfsfsffsfsfsfsfsfs";
                //var max= Regex.Matches(input, patter).Count;

                // input="sdf1sfsf4fsfsfsf3sfsfs";
                // max= Regex.Matches(input, patter).Count;
                // input="sdfs4fsff399sfsfsfs7fsfs";
                // max= Regex.Matches(input, patter).Count;
                // input="sdfs999fsffsfsfsfs00fsfs";
                // max= Regex.Matches(input, patter).Count;
                //teseq();
                //testhtml();
                // test(); 
                //test1();
                //CRD_CD_CreditUserInfoBusiness bus = new CRD_CD_CreditUserInfoBusiness();
                //BridgingBusiness b = new BridgingBusiness();
                //bus.GetStateBycert_no("330725198307190223", 30);
                //var buss = bus.GetList();
                //bus.GetAllListByConfig("0", "20170710");
                //bus.GetAllListByCert_no("330725198307190223");
                //List<CRD_CD_CreditUserInfoEntity> list = new List<CRD_CD_CreditUserInfoEntity>();
                //list.Add(new CRD_CD_CreditUserInfoEntity() { Cert_No = "330725198307190223", Error_Reason = "sdf", State = 5 });
                //bus.UpdateListStateByCert_No(list);
                //bus.isInLimitedDays(30, "330725198307190223");

                //BaseDao dao = new BaseDao();
                //var list = dao.Select<CRD_CD_CreditUserInfoEntity>("select top 8 * from credit.CRD_CD_CreditUserInfo");
                //new CRD_CD_CreditUserInfoBusiness().UpdateState(list, (byte)RequestState.UpLoadSuccess);
                //s.isInLimitedDays(30, "220381197903180442");
                //var fiel = System.IO.File.ReadAllText(@"D:\vcData\91UU\00100101142\RecvFile\郝亮_100101153\410224198804212039.html", Encoding.GetEncoding("GBK"));
                //new Analisis().SaveData("", fiel);
                //Console.Read();
                // Console.Read();
                //testsummary();

                Console.WriteLine("结束");
                Console.Read();

            }
            static void testsummary()
            {
                // var re=  PostDataToUrl("http://10.1.12.73:8002/credit/query/summary", "{\"ReportSn\":\"2014071800001744985888\"}");
                var re = PostDataToUrl("http://localhost:40689/credit/query/summary", "{\"ReportSn\":\"2016082300003119496352\"}");
                var res = JsonConvert.DeserializeObject<BaseRes>(re);

            }
            static void teseq()
            {
                var r = new CRD_CD_CreditUserInfoBusiness().GetAllList();
                List<CRD_CD_CreditUserInfoEntity> clist = new List<CRD_CD_CreditUserInfoEntity>();
                List<CRD_CD_CreditUserInfoEntity> list = new List<CRD_CD_CreditUserInfoEntity>();
                clist.Add(new CRD_CD_CreditUserInfoEntity() { Cert_No = "sdf", LocalDirectoryName = "sdfsfsdf" });
                clist.Add(new CRD_CD_CreditUserInfoEntity() { Cert_No = "s2df", LocalDirectoryName = "sdfs33fsdf" });
                clist.Add(new CRD_CD_CreditUserInfoEntity() { Cert_No = "sddsdf", LocalDirectoryName = "sdfs43dfsdf" });
                list.Add(new CRD_CD_CreditUserInfoEntity() { Cert_No = "sdf", LocalDirectoryName = "sdfsfsddffdff" });
                list.Add(new CRD_CD_CreditUserInfoEntity() { Cert_No = "sd3434343434f", LocalDirectoryName = "sdfsfsdf" });
                var result = clist.Except(list, new CreditEquelityCompare()).ToList();
                Console.Read();
            }
        }
        class Mapper
        {
            public string batchCount { get; set; }
            public string batchNo { get; set; }
        }
        class Maper
        {
            public string batchCount { get; set; }
            public string batchNo { get; set; }

            public Dictionary<string, string> errorMap { get; set; }
        }
        class request
        {
            public string txCode { get; set; }
            public string reqDate { get; set; }
            public string reqTime { get; set; }
            public string token { get; set; }

            public string reqSerial { get; set; }
            public string content { get; set; }

        }
        class person
        {
            public int? age { get; set; }
            public string name { get; set; }

        }
        class person1
        {
            public string name { get; set; }
            public string name1 { get; set; }
        }
        class CreditEquelityCompare : IEqualityComparer<CRD_CD_CreditUserInfoEntity>
        {

            public bool Equals(CRD_CD_CreditUserInfoEntity x, CRD_CD_CreditUserInfoEntity y)
            {
                if (x.Cert_No == y.Cert_No && x.LocalDirectoryName != y.LocalDirectoryName)
                {
                    return true;
                }
                return false;
            }

            public int GetHashCode(CRD_CD_CreditUserInfoEntity obj)
            {
                return obj.ToString().GetHashCode();
            }
        }

    
}
