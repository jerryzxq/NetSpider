using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Common.Ext;

namespace Vcredit.Common.Helper
{
    public class HttpHelper
    {
        #region 变量
        private string _ProxyIp = string.Empty;
        #endregion

        #region 构造函数
        public HttpHelper(string ProxyIp)
        {
            _ProxyIp = ProxyIp;
        }
        public HttpHelper()
        {
        }
        #endregion

        #region 执行request请求
        public HttpResult GetHtml(HttpItem item)
        {
            //返回参数
            HttpResult _result = new HttpResult();
            HttpHelper_2 _httphelper = new HttpHelper_2();

            if (!_ProxyIp.IsEmpty())
            {
                item.ProxyIp = _ProxyIp;
            }
            do
            {
                _result = _httphelper.GetHtml(item);
                if (_result.StatusCode == 0)
                {
                    item.ReconnectCount--;
                    System.Threading.Thread.Sleep(4000);
                }
                else
                {
                    item.ReconnectCount = 0;
                }
            }
            while (item.ReconnectCount > 0);
            return _result;
        }
        #endregion
    }


}
