using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Mongo.ProvidentFund
{
    public class FormSetting
    {
        /// <summary>
        /// 登录类别
        /// </summary>
        public int LoginType { get; set; }
        /// <summary>
        /// 是否需要验证码
        /// </summary>
        public int HasVerCode { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        private List<FormParam> _FormParams = new List<FormParam>();
        /// <summary>
        /// 对应表单信息
        /// </summary>
        public List<FormParam> FormParams
        {
            get { return _FormParams; }
            set { _FormParams = value; }
        }
    }
}
