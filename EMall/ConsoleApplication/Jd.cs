using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Vcredit.NetSpider.Emall.Crawler.JingDong;
using Vcredit.Common.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.Common.Utility;
using System.Threading;
using System.Drawing;
using Vcredit.Framework.Queue.Redis;
using Vcredit.NetSpider.Emall.Dto.TaoBao;
using Vcredit.Framework.Server.Dto;
using Vcredit.NetSpider.Emall.Dto;
using System.Text.RegularExpressions;
using NSoup.Nodes;
using NSoup;
using Vcredit.NetSpider.Emall.Entity;
using Vcredit.NetSpider.Emall.Framework;
using Vcredit.NetSpider.Emall.Processor.Impl;
using Vcredit.NetSpider.Emall.Framework.Utility;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Emall.Crawler.JingDong.Mobile;
namespace ConsoleApplication
{
    class CrawlerType
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        public const string userinfo = "1";
        /// <summary>
        /// 白条信息
        /// </summary>
        public const string baitiao = "2";
        /// <summary>
        /// 购物车信息
        /// </summary>
        public const string shoppingCart = "3";
        /// <summary>
        /// 收货地址
        /// </summary>
        public const string receiveAddress = "4";
        /// <summary>
        /// 分享信息
        /// </summary>
        public const string shareDetail = "5";
        /// <summary>
        /// 快捷支付信息
        /// </summary>
        public const string quickPayment = "6";
        /// <summary>
        /// 订单信息
        /// </summary>
        public const string orderList = "7";
        /// <summary>
        /// 成长值信息
        /// </summary>
        public const string growthValueDetail = "8";
        /// <summary>
        /// 消费信息
        /// </summary>
        public const string expenseRecord = "9";
        /// <summary>
        /// 浏览历史信息
        /// </summary>
        public const string browseHistory = "A";
        /// <summary>
        ///关注活动信息
        /// </summary>
        public const string focusActity = "B";
        /// <summary>
        /// 关注品牌信息
        /// </summary>
        public const string focusBrand = "C";
        /// <summary>
        /// 关注产品信息
        /// </summary>
        public const string focusProduct = "D";
        /// <summary>
        /// 关注商铺信息
        /// </summary>
        public const string focusShop = "E";

    }
    class Program1
    {
        //static IEmallExecutor jingDongExecutor = ExecutorManager.GetJingDongExecutor();
        static string PostDataToUrl(string url, string data)
        {
            HttpItem httpItem = new HttpItem()
            {
                URL = url,
                Method = "POST",
                Encoding = Encoding.UTF8,
                Postdata = data
            };
            HttpResult httpResult = new HttpHelper().GetHtml(httpItem);
            return httpResult.Html;

        }
        static string PostDataToUrl(string url)
        {
            HttpItem httpItem = new HttpItem()
            {
                URL = url,
                Method = "GET",
                Encoding = Encoding.UTF8,
            };
            HttpResult httpResult = new HttpHelper().GetHtml(httpItem);
            return httpResult.Html;

        }
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="binData"></param>
        /// <param name="fileName">文件名（虚拟路径）</param>
        public static void SaveFile(byte[] binData, string fileName)
        {
            FileStream fileStream = null;
            MemoryStream m = new MemoryStream(binData);
            try
            {
                //  fileName = HttpContext.Current.Server.MapPath(fileName);
                fileStream = new FileStream(fileName, FileMode.Create);
                m.WriteTo(fileStream);
            }
            catch (Exception)
            {
                //Log4netAdapter.WriteError(fileName + "读取时发生异常", ex);
                //throw;
            }
            finally
            {
                m.Close();
                fileStream.Close();
            }
        }
        public static void GetJingDong()
        {


            EmallReq regReq = new EmallReq();

            regReq.Username = "13918352173";
            regReq.Password = "456chehaoasd";
            regReq.IdentityCard = "11113434s3er";
            regReq.Name = "ww";
            var Username = new { Username = "13918352173" };

            string result1 = PostDataToUrl("http://10.138.60.43:8100/JingDong/init", JsonConvert.SerializeObject(Username).ToBase64());

            var res = JsonConvert.DeserializeObject<VerCodeRes>(result1);

            if (res.VerCodeBase64 != "none")
            {
                SaveFile(Convert.FromBase64String(res.VerCodeBase64), @"D:\京东数据采集\验证码图片\" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".jpg");
                Console.Write("请输入验证码：");
                regReq.Vercode = Console.ReadLine();
            }
            regReq.Token = res.Token;
            string jsong = JsonConvert.SerializeObject(regReq);

            string result = PostDataToUrl("http://10.138.60.43:8100/JingDong/login", jsong.ToBase64());
            //string result = PostDataToUrl("http://10.100.26.48:8080/JingDong/login", jsong);
            //  Res = jingdong.EmallLogin(regReq);

        }

        static void AnalysisJson()
        {
            string json = File.ReadAllText(@"D:\json.txt");
            string resultStr = CommonFun.GetMidStr(json, "\"resData\":", "}})");
            var jtoken = JObject.Parse(resultStr);
            Dictionary<string, myjson> dic = new Dictionary<string, myjson>();
            foreach (var item in jtoken)
            {
                var results = JsonConvert.DeserializeObject<List<myjson>>(item.Value.ToString());
                // dic.Add(results.erpOrderId, results);

            }
        }

        /// <summary>
        /// 通过节点获取该节点的字符串组
        /// </summary>
        /// <param name="JsonString">json字符串源</param>
        /// <param name="SelectNode">选择的节点</param>
        /// <returns></returns>
        public static List<string> GetArrayFromParse(string JsonString, string SelectNode)
        {
            List<string> list = new List<string>();

            try
            {
                JObject jObject = JObject.Parse(JsonString);
                object obj = jObject[SelectNode];
                if (obj != null)
                {
                    JArray jArray = JArray.Parse(obj.ToString());
                    for (var i = 0; i < jArray.Count; i++)
                    {

                        list.Add(jArray[i].ToString());
                    }
                }
                return list;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        static void TextJingDongQuery()
        {

            //http://localhost:1129/
            var token = new { Token = "ea60c1bc52164c84aac4740ab880cd55" };
            string result1 = PostDataToUrl("http://localhost:1129/JingDong/QueryUserInfo/Json", JsonConvert.SerializeObject(token));

            string result = PostDataToUrl("http://localhost:1129/JingDong/QueryOrderInfo/Json", JsonConvert.SerializeObject(token));

        }
        public static bool CheckAuthcode()
        {
            //判断是否需要验证 返回Json({"verifycode":false})
            string user = "2609486706@qq.com";
            string r = new Random().NextDouble().ToString();
            string url = "https://passport.jd.com/uc/showAuthCode?r=" + r + "&version=2015";//请求地址
            string res = string.Empty;//请求结果,请求类型不是图片时有效
            string pdata = "loginName=" + Uri.EscapeUriString(user).Replace("@", "%40");//提交数据(必须项)
            Wininet wnet = new Wininet();
            //res = wnet.PostUtf8(url, pdata); //使用UTF8编码提交
            res = wnet.PostData(url, pdata); //使用gbk编码提交
            Console.WriteLine(res);
            if (Common.QuChuZhongJian(res, "verifycode\":", "}") == "false")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public static void ChuShiHua()
        {
            int jishu = 0;
            do
            {
                jishu++;
                if (jishu > 1)
                {
                    //初始化失败再次尝试

                    Thread.Sleep(500);
                }
                string yuanMa = string.Empty;//请求结果,请求类型不是图片时有效
                Wininet wnet = new Wininet();
                yuanMa = wnet.GetData("https://passport.jd.com/new/login.aspx");
                //获取uuid
                Common.uuid = Common.QuChuZhongJian(yuanMa, "id=\"uuid\" name=\"uuid\" value=\"", "\"");
                //获取两个hidden
                string hiddenaft = Common.QuChuZhongJian(yuanMa.Replace(" ", ""), "_t\"id=\"token\"value=\"", "\"");
                Common.hidden = "_t=" + hiddenaft;
                string hidden2bef = Common.QuChuZhongJian(yuanMa.Replace(" ", ""), "\"" + hiddenaft + "\"class=\"hide\"/>\r\n\r\n<inputtype=\"hidden\"name=\"", "\"");
                string hidden2aft = Common.QuChuZhongJian(yuanMa.Replace(" ", ""), hidden2bef + "\"value=\"", "\"");
                Common.hidden2 = hidden2bef + "=" + hidden2aft;
            }
            while (Common.uuid == "" || Common.hidden == "" || Common.hidden2 == "");
            //this.SetText("\r\n[" + DateTime.Now.ToString("HH:mm:ss") + "]初始化成功", textBox3);
        }
        public static byte[] GetByteImage(Image img)
        {

            byte[] bt = null;

            if (!img.Equals(null))
            {
                using (MemoryStream mostream = new MemoryStream())
                {
                    Bitmap bmp = new Bitmap(img);

                    bmp.Save(mostream, System.Drawing.Imaging.ImageFormat.Jpeg);//将图像以指定的格式存入缓存内存流

                    bt = new byte[mostream.Length];

                    mostream.Position = 0;//设置留的初始位置

                    mostream.Read(bt, 0, Convert.ToInt32(bt.Length));

                }

            }
            return bt;
        }
        static void tests()
        {
            ChuShiHua();
            string user = "mouse0809";
            string PWD = "A3472346";
            string yuanMa;
            string cookie;
            string url;
            Wininet wnet = new Wininet();
            //do
            //{
            bool mustCode = CheckAuthcode();
            if (mustCode)
            {
                string Getauthorcodeurl = "https://authcode.jd.com/verify/image?a=1&acid=" + Common.uuid + "&uid=" + Common.uuid + "&yys=" + Common.GetTimeStamp(false);//请求地址

                Wininet codewnet = new Wininet();
                Image mage = new Bitmap(codewnet.GetImage(Getauthorcodeurl));

                mage.Save(@"D:\京东数据采集\验证码图片\" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".jpg");
                // FileOperateHelper.WriteVerCodeImage(DateTime.Now.ToString("yyyyMMddhhmmss"), GetByteImage(mage));
                Console.Write("请输入验证吗：");
                Common.codeAns = Console.ReadLine();
            }
            else
            {
                Common.codeAns = "";
            }
            Random rd = new Random();
            string putData = "uuid=" + Common.uuid + "&machineNet=&machineCpu=&machineDisk=&eid=&fp=&" + Common.hidden + "&" + Common.hidden2 + "&loginname=" + Uri.EscapeUriString(user).Replace("@", "%40") + "&nloginpwd=" + PWD.Replace("@", "%40").Replace("+", "%2B") + "&loginpwd=" + PWD.Replace("@", "%40").Replace("+", "%2B") + "&authcode=" + Common.codeAns;

            url = "https://passport.jd.com/uc/loginService?uuid=" + Common.uuid + "&&r=" + rd.NextDouble().ToString() + "&version=2015";//请求地址
            yuanMa = string.Empty;//请求结果,请求类型不是图片时有效
            //res = wnet.PostUtf8(url, pdata); //使用UTF8编码提交
            yuanMa = wnet.PostData(url, putData); //使用gbk编码提交
            cookie = wnet.GetCookies(url);
            Console.WriteLine(yuanMa);

            //if (yuanMa.IndexOf("\\u8bf7\\u8f93\\u5165\\u9a8c\\u8bc1\\u7801") > -1)
            //{
            //    ChuShiHua();
            //}
            //}
            //while (yuanMa.IndexOf("\\u8bf7\\u8f93\\u5165\\u9a8c\\u8bc1\\u7801") > -1);
            if (yuanMa.IndexOf("success") < 0)
            {
                if (yuanMa.IndexOf("\"_t\"") > -1)
                {
                    Common.hidden = "_t=" + Common.QuChuZhongJian(yuanMa, "\"_t\":\"", "\"");
                }
                if (yuanMa.IndexOf("username") > -1)
                {
                    string extext = Common.UnitoChse(Common.QuChuZhongJian(yuanMa, "username\":\"", "\""));

                }
                else if (yuanMa.IndexOf("pwd") > -1)
                {
                    string extext = Common.UnitoChse(Common.QuChuZhongJian(yuanMa, "pwd\":\"", "\""));

                }
                else if (yuanMa.IndexOf("emptyAuthcode") > -1)
                {
                    string extext = Common.UnitoChse(Common.QuChuZhongJian(yuanMa, "emptyAuthcode\":\"", "\""));

                }
                else
                {

                }
                // 启动初始化线程
                //Thread t = new Thread(ChuShiHua);
                //t.Start();
            }
            else
            {
                //this.SetText("\r\n[" + DateTime.Now.ToString("HH:mm:ss") + "]登录成功", textBox3);
                //this.SetText("\r\n[" + DateTime.Now.ToString("HH:mm:ss") + "]Cookies:" + cookie, textBox3);
            }
        }
        static void t()
        {
            spd_applyEntity apply = new spd_applyEntity() { Crawl_status = 2002 };
            if (apply.Crawl_status == (int)SystemEnums.CrawlStatus.数据正在采集 || apply.Crawl_status == (int)SystemEnums.CrawlStatus.数据采集成功)
            {
                Console.WriteLine();
            }

        }
        static void test6()
        {

            //  string res = PostDataToUrl("http://10.138.60.43:8200/jingDong/QueryJDWhiteBarOrderInfo/8d1a13bedee246bdb0a830163a7910eb");
            string token = "{\"Token\":\"014899a5207d4b8aaf45f474b458877f\"}";
            var res = PostDataToUrl("http://10.138.60.43:7777/emall/crawler/jingDong/crawlstatus", token.ToBase64());
            //  var res = PostDataToUrl("http://10.138.60.43:8200/jingDong/crawlstatus", token.ToBase64());

            res = PostDataToUrl("http://10.138.60.43:8200/jingDong/BaiTiao/Repayment/75ef96f272ed41e的3bd7b3f485e9baedd");
            //var resentity =JsonConvert.DeserializeObject<BaseRes>(res);
            //if(resentity.Result!=null)
            //var list = JsonConvert.DeserializeObject<List<WhiteAllOrderEntity>>(resentity.Result);

            // res = PostDataToUrl("http://10.138.60.43:8200/jingDong/QueryJDWhiteBarInstalmentInfo/19753是998822");

        }
       public  static void testMobileCrawler()
        {
            string url = "http://localhost:65458/JingDong/cookieCrawler";
            string urlinit = "http://localhost:65458/JingDong/cookieinit";
            //string url = "http://10.138.60.43:8100/Jingdong/cookieCrawler";
            //string urlinit = "http://10.138.60.43:8100/Jingdong/cookieinit";
            EmallReq req = new EmallReq()
            {
               // Cookies = "JAMCookie=true; abtest=20170619160911537_55; mobilev=html5; sid=eba908b1022c996ff76d84cccd4efb8e; USER_FLAG_CHECK=4d90029fb2cb0f24095c4a2fea8f81a6; returnurl=\"http://home.m.jd.com/myJd/home.action?sid=eba908b1022c996ff76d84cccd4efb8e\"; shshshfpa=01869338-8271-5563-835d-157813e4ca3d-1497859751; __jda=122270672.14978597519141380602470.1497859751.1497859751.1497859751.1; __jdb=122270672.1.14978597519141380602470|1.1497859751; __jdv=122270672|direct|-|none|-|1497859751921; __jdc=122270672; mba_muid=14978597519141380602470; mba_sid=14978597519285044742837305397.1; shshshfpb=1d1ea97e652ed42e0a5e8ff35bf3889aa69d049d807c066a3594786a8f; __jdu=14978597519141380602470; TrackerID=PXfOS4LaQPoRl9Yfy3a0uaczVtkLkWWKdBzyfifFMBSMeTy2TzviIrrYw-7S8j2TPvfHOo5fMoAWzJtIQ2FLN3RdFL6BLthAKepmTTgkrfM; pinId=6owf6JDRRmY; pt_key=AAFZR4bAADD0YEoBHgWI25hrh_77xJEl1ba-szq4VGj4UDA8A86j3qhDV3BSFj3f1gELh0ownW8; pt_pin=lu8666; pt_token=pisx969v; pwdt_id=lu8666; whwswswws=aUSMcdXUdAe5dhTrj%2FbttscCtv7v93lHwGKBABGIf1%2FdOehwdHDMJQ%3D%3D",
               Cookies ="JAMCookie=true; abtest=20170620150826868_04; mobilev=html5; sid=67cf0f1cb8702ec9e655f9697aa1e649; USER_FLAG_CHECK=939ad4334c48c349a922d3229c2d7722; returnurl=\"http://home.m.jd.com/myJd/home.action?sid=67cf0f1cb8702ec9e655f9697aa1e649\"; shshshfpa=86d5af6c-84a0-45d6-5d9a-d06abb5844ed-1497942505; __jda=122270672.14979425058331942758508.1497942505.1497942505.1497942505.1; __jdb=122270672.1.14979425058331942758508|1.1497942505; __jdv=122270672|direct|-|none|-|1497942505840; __jdc=122270672; mba_muid=14979425058331942758508; mba_sid=14979425058477585526068610879.1; __jdu=14979425058331942758508; shshshfpb=08bd966ceb608b24ef2463e215fa6415492f2b2ac4c03c5ce5948c9eb9; TrackerID=HcAC-FBKVEoiADRWYNJVovq5NvKG88hODVDdhtZj7_-FFYLIsHkvGspHegoloexq-Ca_zJ3Tt7Evdb6bvQpsfSoaRii76mAdk8Ll1A6FsMg; pinId=7FwKMdBAkDPbkBM-UXW6NA; pt_key=AAFZSMqdADCn1U1keB59x6N07FOrNdmmCbHrZgICPpkGkpGkwF-4ONPgd-Q5nuEEv-dVplDEmXo; pt_pin=yejiangliang; pt_token=nx6a9fhq; pwdt_id=yejiangliang; whwswswws=6sy5iuAqE3L4LihhZ7Qmdy8UaNfM%2FNr3pZa12sieqFsrcJhHl7aN5w%3D%3D",
               Username = "2609486706@qq.com",
                Password = "15806033abc123",
                BusType = "test"
            };
            req.Token = JsonConvert.DeserializeObject<BaseRes>(PostDataToUrl(urlinit, "sdfsfsfsf")).Token;
            var result = PostDataToUrl(url, JsonConvert.SerializeObject(req).ToBase64());

            //Vcredit.NetSpider.Emall.Crawler.JingDong.Mobile.MobileJingDongCrawler crawler = new Vcredit.NetSpider.Emall.Crawler.JingDong.Mobile.MobileJingDongCrawler();
            //crawler.EmallCrawler(null);
        }



        public static void testjd()
        {
            string username = "lu8666";
            string password = "lvxiu1325999";
            JingDongCrawler jd = new JingDongCrawler();
            //  var res = jd.EmallInit(new EmallReq() { Username = username });
            var ress = PostDataToUrl("http://10.138.60.49:7221/JingDong/init", JsonConvert.SerializeObject(new { Username = username }).ToBase64());
            var res = JsonConvert.DeserializeObject<VerCodeRes>(ress);
            string vercode = null;

            if (res.VerCodeBase64 != "none")
            {
                SaveFile(Convert.FromBase64String(res.VerCodeBase64), @"D:\京东数据采集\验证码图片\" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".jpg");
                Console.Write("请输入验证码：");
                vercode = Console.ReadLine();
            }
            var req = new EmallReq() { Username = username, Password = password, Token = res.Token, Vercode = vercode };
           // var reses = PostDataToUrl("http://10.138.60.49:7221/JingDong/login", JsonConvert.SerializeObject(req).ToBase64());
            // string cookies = RedisHelper.GetCache(res1.Token + "Str").ToString();
            //   req.Cookies = cookies.Replace(",", "");

            //            req.Cookies = @"__jda=122270672.1564088183.1472544813743.1472544813743.1472544813743.1; __jdb=122270672.1.1564088183|1.1472544813743; __jdv=122270672|direct|-|none|-; __jdc=122270672; __jdu=1564088183; mba_muid=1564088183; mba_sid=14725448137619809997792147900.1; TrackerID=_LLwbZ63P3QDgUdFgxWZgvFD0AUrfDf15Yptv11G6w5lp4k_tyEZuXEKyZyBsJ8ZZnvUf4--1bETRmruypnk4hTuFdR_DHFX7W_7P2Owi8Bkgh_POeDj5euLYqWUTtPb47Nmj1myLiq6pMjrPgm-Tg; pinId=NCwDtUBPsKT76Oz1LCRIkrV9-x-f3wj7; pt_key=AAFXxUBFADAcAfkinAxm1k3iIY46nfYA1v3SyVr8VAtgWNmy1b42t1j
            //KYCncjUSi-tT64cBa4eg; pt_pin=436968153-531651; pt_token=mb5a7b53; pwdt_id=436968153-531651; whwswswws=uM1pc9RmSqcsufKAg22Sti3wUSdTuj2fvzpcq%2FNPbsDRrk8qNTgq%2FA%3D%3D; mobilev=html5; sid=a6426aead6d91c2e1bbc56ca519c2495; USER_FLAG_CHECK=312309fadb694428d8cbfe98959cf73c; abtest=20160830161357565_50; returnurl='https://m.jd.com?sid=a6426aead6d91c2e1bbc56ca519c2495'";
            // new  JingDongExecutor().EmallLoginForCookies(req);  

            //string result1 = PostDataToUrl("http://10.138.60.43:8100/JingDong/UseCookieDataCrawler", JsonConvert.SerializeObject(req).ToBase64());
            jd.EmallCrawler(new EmallReq() { Username = username, Password = password, Token = res.Token });
        }
       public  static void testjd1()
        {
            //string username = "2609486706@qq.com";
            //string password = "15806033abc123";
            string username = "lu8666";
            string password = "lvxiu1325999";
            JingDongCrawler jd = new JingDongCrawler();
            var res = jd.EmallInit(new EmallReq() { Username = username });
            string vercode = null;
            if (res.VerCodeBase64 != "none")
            {
                SaveFile(Convert.FromBase64String(res.VerCodeBase64), @"D:\京东数据采集\验证码图片\" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".jpg");
                Console.Write("请输入验证码：");
                vercode = Console.ReadLine();
            }
            var req = new EmallReq() { Username = username, Password = password, Token = res.Token, Vercode = vercode };
            var res1 = jd.EmallLogin(req);
            // string cookies = RedisHelper.GetCache(res1.Token + "Str").ToString();RedusGekoer,GetCache(res1.Token+"Str");
            //   req.Cookies = cookies.Replace(",", ""); 

                   //     req.Cookies = @"__jda=122270672.1564088183.1472544813743.1472544813743.1472544813743.1; __jdb=122270672.1.1564088183|1.1472544813743; __jdv=122270672|direct|-|none|-; __jdc=122270672; __jdu=1564088183; mba_muid=1564088183; mba_sid=14725448137619809997792147900.1; TrackerID=_LLwbZ63P3QDgUdFgxWZgvFD0AUrfDf15Yptv11G6w5lp4k_tyEZuXEKyZyBsJ8ZZnvUf4--1bETRmruypnk4hTuFdR_DHFX7W_7P2Owi8Bkgh_POeDj5euLYqWUTtPb47Nmj1myLiq6pMjrPgm-Tg; pinId=NCwDtUBPsKT76Oz1LCRIkrV9-x-f3wj7; pt_key=AAFXxUBFADAcAfkinAxm1k3iIY46nfYA1v3SyVr8VAtgWNmy1b42t1j
            //KYCncjUSi-tT64cBa4eg; pt_pin=436968153-531651; pt_token=mb5a7b53; pwdt_id=436968153-531651; whwswswws=uM1pc9RmSqcsufKAg22Sti3wUSdTuj2fvzpcq%2FNPbsDRrk8qNTgq%2FA%3D%3D; mobilev=html5; sid=a6426aead6d91c2e1bbc56ca519c2495; USER_FLAG_CHECK=312309fadb694428d8cbfe98959cf73c; abtest=20160830161357565_50; returnurl='https://m.jd.com?sid=a6426aead6d91c2e1bbc56ca519c2495'";
            // new  JingDongExecutor().EmallLoginForCookies(req);  

          //  string result1 = PostDataToUrl("http://10.138.60.43:8100/JingDong/UseCookieDataCrawler", JsonConvert.SerializeObject(req).ToBase64());
            jd.EmallCrawler(new EmallReq() { Username = username, Password = password, Token = res.Token });
        }

        private static void JingDongExecutor()
        {
            throw new NotImplementedException();
        }
        //public static  void SaveFile(byte[] binData, string fileName)
        //{
        //    FileStream fileStream = null;
        //    MemoryStream m = new MemoryStream(binData);
        //    try
        //    {
        //        //  fileName = HttpContext.Current.Server.MapPath(fileName);
        //        fileStream = new FileStream(fileName, FileMode.Create);
        //        m.WriteTo(fileStream);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log4netAdapter.WriteError(fileName + "读取时发生异常", ex);
        //        throw;
        //    }
        //    finally
        //    {
        //        m.Close();
        //        fileStream.Close();
        //    }
        //}
        private static void SendTALoginQueue()
        {
            var entity = new EmallReq
            {
                Token = Guid.NewGuid().ToString("N"),
                Username = "18516291436",
                Password = "1qazXDR%",
            };
            var queueName = "Test_Redis_Queue";
            RedisQueue.Send<EmallReq>(entity, queueName);
        }


        [Serializable]
        public class myjson
        {
            [JsonProperty(PropertyName = "contact", NullValueHandling = NullValueHandling.Ignore)]
            public string contactName { get; set; }
            [JsonProperty(PropertyName = "OrderI", NullValueHandling = NullValueHandling.Ignore)]
            public string erpOrderId { get; set; }
            public string orderStatus { get; set; }
            public string price { get; set; }
            public string productName { get; set; }
            public string payType { get; set; }
            public string mobile { get; set; }
            public string productUrl { get; set; }
            public string quantity { get; set; }



        }
        public enum Emall
        {

            Alipay
        }

    }

}
