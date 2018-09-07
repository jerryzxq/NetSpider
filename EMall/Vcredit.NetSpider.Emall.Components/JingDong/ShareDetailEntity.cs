using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using ServiceStack.DataAnnotations;
using Vcredit.Common.Ext;
namespace   Vcredit.NetSpider.Emall.Entity 
{
    [Alias("JD_ShareDetail")]
    
    public class ShareDetailEntity : BaseEntity  //分享详细
    {

     
        /// <summary>
        /// 分享网站名称
        /// </summary>
   
        public string WebSiteName { get; set; }

        /// <summary>
        /// 分享网站用户昵称
        /// </summary>
   
        public string NickName { get; set; }
        [Ignore]
        public string ShareValidity { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        [Alias("ShareValidity")]
        public DateTime? ShareValidityReal { get { return ShareValidity.ToDateTime(); } }


    }
}
