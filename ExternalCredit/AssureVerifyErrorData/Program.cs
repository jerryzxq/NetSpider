using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;

namespace AssureVerifyErrorData
{
    class Program
    {
        static string cookies = "JSESSIONID=gQwCZV1HQJwq78ThGpdyL6TdB96TFJRrbLJLhJnyrZl1tk2TnNxf!-2129398578;BIGipServerpool_xwqy=XkwuYGeFDbe+po9thvBvfz88UKK2mkx3TVOtsOje00opFuK4QKrqMyAjE2x1TjZYyYgMwgbESy3JznI=;ipcrs=bs/iKqtmQzaCA7Sh8Abp1S+OUcJ3Ck6gmHGeU8uU3hDiD5aA/sABDZfO/bvm/nu0YXpKceCZwShn";

        static HttpHelper httpHelper = new HttpHelper();

        static void Main(string[] args)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
 ((sender, certificate, chain, sslPolicyErrors) => true);

            VerifyErrorData();
        }

        #region 异常数据核实

        /// <summary>
        /// 异常数据核实
        /// </summary>
        private static void VerifyErrorData()
        {
            var limit = 20;
            var startPage = 1538;
            for (int page = startPage; page >= 1; page--)
            {
                var url =
                    string.Format("https://msi.pbccrc.org.cn/sfcp/exdata/orgex/gb/list?_dc=1494898254625&scode=&page={0}&start={1}&limit={2}", page, (page - 1) * limit, limit);

                var errorDataList = GetErrorData(url);

                if (errorDataList != null &&
                    errorDataList.root != null &&
                    errorDataList.root.Any() &&
                    errorDataList.root.Where(x => x.istate == 0).FirstOrDefault() != null)
                {
                    var opData = errorDataList.root.Where(x => x.istate == 0).ToList();
                    foreach (var d in opData)
                    {
                        Log4netAdapter.WriteInfo(string.Format("业务号：{0} 开始处理=======", d.guaranteelettercode));
                        Verify(d);
                        Log4netAdapter.WriteInfo(string.Format("业务号：{0} 处理结束=======", d.guaranteelettercode));

                        Thread.Sleep(500);
                    }
                }
            }
        }
        private static void Verify(ErrorRoot d)
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
        private static ErrorRootobject GetErrorData(string url)
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
