using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.ModelLayer.Common;

namespace Vcredit.ExternalCredit.CrawlerLayer.Assure.Handlers
{
    /// <summary>
    /// 责任链基类
    /// </summary>
    public abstract class BaseCreditQueryHandler
    {
        private BaseCreditQueryHandler _nextHandler;

        public void SetNextHandler(BaseCreditQueryHandler nextHandler)
        {
            this._nextHandler = nextHandler;
        }

        public HandleResponse HandleRequest(HandleRequest request)
        {
            var res = this.Handle(request);

            if (res.DoNextHandler && this._nextHandler != null)
            {
                this.SetNextRequest(request, res);
                res = this._nextHandler.HandleRequest(request);
            }

            return res;
        }

        protected abstract void SetNextRequest(HandleRequest request, HandleResponse res);

        protected abstract HandleResponse Handle(HandleRequest request);
    }

    public class HandleResponse
    {
        private bool _doNextHandler = false;
        public bool DoNextHandler
        {
            get { return _doNextHandler; }
            set { _doNextHandler = value; }
        }

        private dynamic _data = null;
        public dynamic Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public string Description { get; set; }

        public HandlerErrorReason? ErrorReason { get; set; }
    }

    public enum HandlerErrorReason
    {
        /// <summary>
        /// 登陆失败
        /// </summary>
        CookieError = 0,
    }

    public class HandleRequest
    {
        private string _currentCookies = string.Empty;
        public string CurrentCookies
        {
            get { return _currentCookies; }
            set { _currentCookies = value; }
        }

        private List<CRD_CD_CreditUserInfoEntity> _userInfoList = null;
        public List<CRD_CD_CreditUserInfoEntity> UserInfoList
        {
            get { return _userInfoList; }
            set { _userInfoList = value; }
        }
    }
}
