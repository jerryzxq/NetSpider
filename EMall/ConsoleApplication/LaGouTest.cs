using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Emall.Framework;
using Vcredit.Common.Ext;
using System.Windows.Forms;
namespace ConsoleApplication
{
    class LaGouTest
    {
           HttpHelper httpHelper = new HttpHelper();
                 CookieCollection cookies = new CookieCollection();
                 string username = "18910905273";
                 string password = "15806033109abc";
                 string vercode = string.Empty;
        public   void Login()
        {
            string username = "18910905273";
            string password = "15806033109abc";
            string vercode = string.Empty;

            var Url = "https://passport.lagou.com/login/login.html?ts=1479190466066&serviceId=lagou&service=https%253A%252F%252Fwww.lagou.com%252F&action=login&signature=56AA48292E64666AB9C74E3149E1E0B2";
               //var  Url = "https://passport.lagou.com/login/login.html?ts=1479111981003&serviceId=lagou&service=http%253A%252F%252Fwww.lagou.com%252Fmycenter%252Fdelivery.html&action=login&signature=9F73738FE2650081C20F912956E8BD9as";
                var  httpItem = new HttpItem()
                {
                    URL = Url,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                var  httpResult = httpHelper.GetHtml(httpItem);
                string loginhtml = httpResult.Html;
              
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                string Getauthorcodeurl ="https://passport.lagou.com/vcode/create?from=register&refresh="+FrameWorkCommon.GetTimeStamp(false);//请求地址
                     
                  
                    var codebase64= getverifyimage(Getauthorcodeurl);
                    if (codebase64 != null)
                    {
                        SaveFile(Convert.FromBase64String(codebase64), @"D:\京东数据采集\验证码图片\" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".jpg");
                        Console.Write("请输入验证码：");
                        vercode = Console.ReadLine();
                    }
                    WebBrowserLogin(username, password, vercode,loginhtml);
              //     // var postdata = "username=18910905273&password=05c684582c26dc2b1511b96110abf92a&request_form_verifyCode="+vercode+"&submit=";
              //      var postdata = "{\"isValidate\":true,\"username\":\""+username+"\",\"password\":\""+password+"\",\"request_form_verifyCode\":\"" + vercode + "\"}";

              //      httpItem = new HttpItem()
              //      {
              //          URL = "https://passport.lagou.com/login/login.json",
              //          Method = "Post",
              //          ContentType = "application/x-www-form-urlencoded; charset=utf-8",
              //          Postdata = "dfsf",
              //          ResultCookieType = ResultCookieType.CookieCollection,
              //          CookieCollection = cookies,
              //        //  Allowautoredirect = true,
              //        //  Encoding = Encoding.UTF8,
              //          Accept = "application/json, text/javascript, */*; q=0.01",
              //      //    Cookie="JSESSIONID=FC8620763BE6FF547E1E0C57941FD118; LGMOID=20161115114149-2C52E99FA7A727547EFE84403B49E839;Hm_lvt_4233e74dff0ae5bd0a3d81c6ccf756e6=1479108926,1479109857,1479111482,1479175221; Hm_lpvt_4233e74dff0ae5bd0a3d81c6ccf756e6=1479181554; _ga=GA1.3.1850563540.1479181554; _gat=1; _ga=GA1.2.1850563540.1479181554; user_trace_token=20161115114554-02aead6a-aae6-11e6-a5f8-525400f775ce; LGSID=20161115114554-02aeaf51-aae6-11e6-a5f8-525400f775ce; PRE_UTM=; PRE_HOST=; PRE_SITE=; PRE_LAND=https%3A%2F%2Fpassport.lagou.com%2Flogin%2Flogin.html%3Fts%3D1479181132461%26serviceId%3Dlagou%26service%3Dhttps%25253A%25252F%25252Fwww.lagou.com%25252F%26action%3Dlogin%26signature%3D34E251AFB319D088D6693832AEBFD7BB; LGRID=20161115114554-02aeb090-aae6-11e6-a5f8-525400f775ce; LGUID=20161115114554-02aeb12b-aae6-11e6-a5f8-525400f775ce",
              //          Host="passport.lagou.com",
              //          Referer="https://passport.lagou.com/login/login.html?ts=1479181132461&serviceId=lagou&service=https%253A%252F%252Fwww.lagou.com%252F&action=login&signature=34E251AFB319D088D6693832AEBFD7BB",
              //          UserAgent="Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.73 Safari/537.36",
              //KeepAlive =true,
                        
                       
              //      };
              //     // httpItem.Header.Add("Connection", "keep-alive");
              //      httpItem.Header.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3");
              //      httpItem.Header.Add("x-requested-with", "XMLHttpRequest");
              //      httpItem.Header.Add("Accept-Encoding", "gzip, deflate, br");
              //      httpItem.Header.Add("X-Anit-Forge-Code", "92616583");
              //      httpItem.Header.Add("X-Anit-Forge-Token", "50ba7164-b3d6-4bb2-a6ba-7ecd4cd06210");
              //      httpResult = httpHelper.GetHtml(httpItem);
              //      string result = httpResult.Html;
  

        }
        WebBrowser webBrowser1 = null;
        public void WebBrowserLogin(string username,string password ,string vercode,string html)
        {
            WebBrowser webBrowser1 = new WebBrowser();

         
            //  this.Dispatcher.BeginInvoke(new Action(() => {  webBrowser1 = new WebBrowser(); }));
            webBrowser1.DocumentCompleted += webBrowser1_DocumentCompleted;
          //  webBrowser1.Url = new Uri("https://passport.lagou.com/login/login.html?ts=1479259334753&serviceId=lagou&service=https%253A%252F%252Fwww.lagou.com%252F&action=login&signature=9F567F6FF9BB621070F552317D1E1679");
            webBrowser1.DocumentText =html;

        

            //HtmlElement btnSubmit = webBrowser1.Document.GetElementById("loginButton");
            //btnSubmit.InvokeMember("click"); 

        }

       private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
       
            var elements = webBrowser1.Document.GetElementsByTagName("input");
            elements[0].SetPropertyValue("value", username);
            elements[1].SetPropertyValue("value", username);
            // var verifyele= webBrowser1.Document.GetElementById("isVisiable_request_form_verifyCode");
            if (string.IsNullOrEmpty(vercode))
            {
                elements[2].SetPropertyValue("value", vercode);
            }

            HtmlElement formLogin = webBrowser1.Document.Forms[0];
            formLogin.InvokeMember("submit");
        }
        public string getverifyimage(string url)
        {
            try
            {
                var  httpItem = new HttpItem
                {
                    Method = "GET",
                    URL = url,
                    CookieCollection = cookies,
                    Referer = "https://passport.lagou.com/login/login.html?ts=1479113420847&serviceId=lagou&service=http%253A%252F%252Fwww.lagou.com%252Fmycenter%252Fdelivery.html&action=login&signature=66C7A972F28E22B3CE9A00FD469168C0",
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.73 Safari/537.36",
                    ResultType = Vcredit.Common.Helper.ResultType.Byte,
                    Host = "passport.lagou.com",
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                var  httpResult = httpHelper.GetHtml(httpItem);

                return CommonFun.GetVercodeBase64(httpResult.ResultByte);
            }
            catch (Exception ex)
            {
     
                return null;
            }

        }
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
    }
}
