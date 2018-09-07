using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using Vcredit.Common.Ext;
using ServiceStack.DataAnnotations;
namespace   Vcredit.NetSpider.Emall.Entity 
{
    [Alias("JD_GrowthValueDetail")]
    
    public class GrowthValueDetailEntity : BaseEntity  //成长值详情
    {
        [ Ignore]
        public string HappenDate { get; set; }

        /// <summary>
        /// 发放/减扣时间
        /// </summary>
        [Alias("HappenDate")]
        public DateTime? HappenDateReal { get { return HappenDate.ToDateTime(); } }

        /// <summary>
        /// 成长值
        /// </summary>
        public int GrowthValue { get; set; }

        /// <summary>
        /// 成长值来源
        /// </summary>
      
       
        public string GrowthValueFrom { get; set; }

        /// <summary>
        /// 来源详情
        /// </summary>
      
      
        public string FromDescription { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string AccountName { get; set; }
    }
}
