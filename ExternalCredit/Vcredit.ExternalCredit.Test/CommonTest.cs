using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using Vcredit.Common.Helper;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using System.Linq;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CommonLayer.helper;

namespace Vcredit.ExternalCredit.Test
{
    [TestClass]
    public class CommonTest
    {
        static string cookies = "JSESSIONID=0W8lhLsSZxZJx1Dfm0JdzvcH1pXT2vncZGqBNLpyJghPTYMFPyd2!766795281;BIGipServerpool_xwqy=4tQU1QnGzDW55rJn8V2JIwSNn8oszv/aqCzwcU+fwhcy1MPhvzQsWo0HJf+X4sC5HsUoXeLP5VE4WxI=";

        static HttpHelper httpHelper = new HttpHelper();

        public CommonTest()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
  ((sender, certificate, chain, sslPolicyErrors) => true);
        }

        [TestMethod]
        public void TestMethod1()
        {
            var obj = new FeedBackErrorRootobject { total = "10000", success = true };
            var token = "1111111111";
            var prefix = "testPrefix";
            RedisHelper.SetCache(token, obj, prefix, 10);
            var o = RedisHelper.GetCache<FeedBackErrorRootobject>(token, prefix);
        }

        [TestMethod]
        public void Test_GetcfcaSignCode()
        {
            for (int i = 0; i < 500; i++)
            {
                Thread t = new Thread(() =>
                {
                    var url = "http://localhost:1800/api/SADKSign/1";
                    var item = new HttpItem
                    {
                        URL = url,
                        Method = "get",
                        ContentType = "application/json"
                    };
                    var helper = new HttpHelper();
                    var result = helper.GetHtml(item);
                    Console.WriteLine(result);
                });
                t.Start();
            }
        }

        #region 担保反馈错误信息导出

        /// <summary>
        /// 担保反馈错误信息导出
        /// </summary>
        [TestMethod]
        public void Exportdata()
        {
            var f = File.CreateText("D:/temp/担保反馈错误数据导出.txt");
            int totalPage = 5162;
            int start = 0;
            for (int page = 1; page <= totalPage; page++)
            {
                var url = string.Format("https://msi.pbccrc.org.cn/sync/msgfeedback/error/list?_dc=1500513365463&page={0}&start={1}&limit=20", page, start);

                var html = RequestToUrl(url);
                var entity = JsonConvert.DeserializeObject<FeedBackErrorRootobject>(html);
                if (entity != null && entity.root != null && entity.root.Any())
                {
                    foreach (var r in entity.root)
                    {
                        var msg = string.Format("{0}	{1}	{2}", r.sremark, r.sbusicode, r.serrorcode);

                        f.WriteLine(msg);
                    }
                }
                start += 20;
                Thread.Sleep(500);
            }
            f.Flush();
            f.Close();
        }
        public static string RequestToUrl(string url)
        {
            var httpItem = new HttpItem()
            {
                URL = url,
                Method = "GET",
                Cookie = cookies,
                Referer = "https://msi.pbccrc.org.cn/sync/msgfeedback/guarantee/error/list?fid=57",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.96 Safari/537.36",
            };
            var httpResult = httpHelper.GetHtml(httpItem);
            return httpResult.Html;
        }

        #endregion

        #region 异常数据核实

        /// <summary>
        /// 异常数据核实
        /// </summary>
        [TestMethod]
        public void VerifyErrorData()
        {
            var limit = 20;
            var startPage = 4328;
            for (int page = startPage; page >= startPage; page--)
            {
                var url =
                    string.Format("https://msi.pbccrc.org.cn/sfcp/exdata/orgex/gb/list?_dc=1494898254625&scode=&page={0}&start={1}&limit={2}", page, (page - 1) * limit, limit);

                var errorDataList = this.GetErrorData(url);

                if (errorDataList != null &&
                    errorDataList.root != null &&
                    errorDataList.root.Any() &&
                    errorDataList.root.Where(x => x.istate == 0).First() != null)
                {
                    var opData = errorDataList.root.Where(x => x.istate == 0).ToList();
                    foreach (var d in opData)
                    {
                        Log4netAdapter.WriteInfo(string.Format("业务号：{0} 开始处理=======", d.guaranteelettercode));
                        this.Verify(d);
                        Log4netAdapter.WriteInfo(string.Format("业务号：{0} 处理结束=======", d.guaranteelettercode));

                        Thread.Sleep(500);
                    }
                }
            }
        }
        private void Verify(ErrorRoot d)
        {
            var url = "https://msi.pbccrc.org.cn/sfcp/exdata/gb/remark/";
            var postdata = string.Format("iid={0}&icenterstate=1&remark=%E4%B8%8A%E6%8A%A5%E4%B8%BB%E4%BD%93%E6%9C%89%E8%AF%AF", d.iid);
            var httpItem = new HttpItem()
            {
                URL = url,
                Method = "POST",
                Postdata = postdata,
                Cookie = cookies,
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                Referer = "https://msi.pbccrc.org.cn/sfcp/exdata/orgex?fid=77",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.96 Safari/537.36",
            };
            var httpResult = httpHelper.GetHtml(httpItem);
            Log4netAdapter.WriteInfo(string.Format("业务号：{0} 处理结束返回html ====》{1}", d.guaranteelettercode, httpResult.Html));
        }

        /// <summary>
        /// 获取元数据
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private ErrorRootobject GetErrorData(string url)
        {
            var httpItem = new HttpItem()
            {
                URL = url,
                Method = "GET",
                Cookie = cookies,
                Referer = "https://msi.pbccrc.org.cn/sfcp/exdata/orgex?fid=77",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.96 Safari/537.36",
            };
            var httpResult = httpHelper.GetHtml(httpItem);
            ErrorRootobject entity = null;
            try
            {
                entity = JsonConvert.DeserializeObject<ErrorRootobject>(httpResult.Html);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError(string.Format("实体序列化异常，请求返回 ====》 {0}", httpResult.Html), ex);
            }
            return entity;
        }


        #endregion
    }

    #region HELP CLASS
    /// <summary>
    /// 反馈错误数据Root
    /// </summary>
    public class FeedBackErrorRootobject
    {
        public string total { get; set; }
        public FeedBackErrorRoot[] root { get; set; }
        public bool success { get; set; }
    }
    public class FeedBackErrorRoot
    {
        public string itype { get; set; }
        public string iid { get; set; }
        public string sorgcode { get; set; }
        public object istate { get; set; }
        public string serrorcode { get; set; }
        public object sloancard { get; set; }
        public string sbusitype { get; set; }
        public string idataid { get; set; }
        public string dinputdate { get; set; }
        public object dbusidate { get; set; }
        public string sbusicode { get; set; }
        public string sremark { get; set; }
        public object sdesc { get; set; }
        public object imsgtype { get; set; }
        public string imsgdetailid { get; set; }
        public object sborrowername { get; set; }
        public object action { get; set; }
        public string columnnameoproccurdate { get; set; }
        public string start { get; set; }
        public string limit { get; set; }
    }


    /// <summary>
    /// 异常数据Root
    /// </summary>
    public class ErrorRootobject
    {
        public long total { get; set; }
        public ErrorRoot[] root { get; set; }
        public bool success { get; set; }
    }
    public class ErrorRoot
    {
        public string scode { get; set; }
        public string iid { get; set; }
        public string sorgcode { get; set; }
        public string sorgname { get; set; }
        public string stoporgcode { get; set; }
        public int istate { get; set; }
        public object susername { get; set; }
        public string sentorgcode { get; set; }
        public string spersonorgcode { get; set; }
        public string ddate { get; set; }
        public string guaranteelettercode { get; set; }
        public string stoporgname { get; set; }
        public object exceptionsum { get; set; }
        public object ddealdate { get; set; }
        public string icenterstate { get; set; }
        public object scontant { get; set; }
        public object ddatestart { get; set; }
        public object ddateend { get; set; }
        public string smoneorgname { get; set; }
        public object action { get; set; }
        public string columnnameoproccurdate { get; set; }
        public string start { get; set; }
        public string limit { get; set; }
    }

    #endregion
}
