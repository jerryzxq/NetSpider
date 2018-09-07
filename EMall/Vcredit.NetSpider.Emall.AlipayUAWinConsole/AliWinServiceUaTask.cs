using Noesis.Javascript;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.Framework.Queue.Redis;
using Vcredit.Framework.Server.Dto;
using Vcredit.NetSpider.Emall.Framework;
using Vcredit.NetSpider.Emall.Framework.Javascript;

namespace Vcredit.NetSpider.Emall.AlipayUAWinConsole
{

    public class AliWinServiceUaTask
    {
       

        public void GetLoginUa()
        {
            RedisQueue.Receive<AliPayWinServiceRes>((x) =>
            {
                switch (x.LoanType)
                {
                    case 1:
                        UaOne(x);
                        break;
                    case 2:
                        GetPassword(x);
                        break;
                }
            }, "AliPayWinUaService");
        }

        public void ActionService(AliPayWinServiceRes x)
        {

        }
        [HandleProcessCorruptedStateExceptions]
        void UaOne(AliPayWinServiceRes x)
        {

            JavascriptContext _javascriptContext = null;
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Reset();
                sw.Start();
                _javascriptContext = new JavascriptContext();
                JavaScriptTestRunner test = new JavaScriptTestRunner();
                //test.Include(JavaScriptLibrary.jQuery_1_7_0_min);
                var js = LoadFromResource<AliWinServiceUaTask>("ua_authcenter_login.js").Replace("{_json_token}", x.ResToken);
                test.jsContext.Add(js);
                _javascriptContext.Run(test.JsString());
                var ua = _javascriptContext.GetParameter("code").ToString();
                Console.WriteLine(string.Format("时间：{0},Token：{1}", DateTime.Now.ToString(), x.Token + ",计算时间：" + sw.ElapsedMilliseconds));

                RedisHelper.InsertCache(x.Token + "_ServiceUa", ua);
                Log4netAdapter.WriteInfo("执行ua第一步加密＝＝＝＝>>token:" + x.Token + ",ua:" + ua + ",计算时间：" + sw.ElapsedMilliseconds + ",时间：" + sw.Elapsed.ToString());
                sw.Stop();
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("执行支付宝AlipayUAWinService报错：" + x.Token, e);
            }
            finally
            {
                if (_javascriptContext != null)
                {
                    _javascriptContext.Dispose();
                    _javascriptContext = null;
                    GC.Collect();
                }
            }

        }


        [HandleProcessCorruptedStateExceptions]
        void GetPassword(AliPayWinServiceRes x)
        {

            JavascriptContext _javascriptContext = null;
            try
            {
                _javascriptContext = new JavascriptContext();
                JavaScriptTestRunner test = new JavaScriptTestRunner();
                test.Include(JavaScriptLibrary.jQuery_1_7_0_min);
                string js1 = LoadFromResource<AliWinServiceUaTask>("security_password.js")
                    .Replace("{password}", x.Password)
                    .Replace("{PK1}", x.PK1)
                    .Replace("{TS1}", x.TS1)
                    .Replace("{sid1}", x.Sid1)
                    .Replace("{PK2}", x.PK2)
                    .Replace("{ksk1}", x.Ksk1)
                    .Replace("{pk3}", x.Pk3)
                    .Replace("{timestamp1}", x.Timestamp1)
                    ;
                test.jsContext.Add(js1);
                _javascriptContext.Run(test.JsString());
                string pw = _javascriptContext.GetParameter("password").ToString();
                RedisHelper.InsertCache(x.Token + "_ServicePassword", pw);
                Log4netAdapter.WriteInfo("执行密码加密＝＝＝＝>>token:" + x.Token + ",password:" + pw);
            }

            catch (Exception e)
            {
                Log4netAdapter.WriteError("执行密码加密js出错：" + x.Token, e);
            }
            finally
            {
                if (_javascriptContext != null)
                {
                    _javascriptContext.Dispose();
                    _javascriptContext = null;
                    GC.Collect();
                }
            }


        }

        public string LoadFromResource<T>(string javascriptFile)
        {

            return File.ReadAllText(Config.Current.JsPath + "/" + javascriptFile);
             
        }

        public void Execute()
        {
            var list = RedisQueueBase.GetQueue<AliPayWinServiceRes>(500, "AliPayWinUaService");
            if (list.Count == 0) return;
            foreach (var item in list)
            {
                ActionService(item);
            }
        }
    }
}
