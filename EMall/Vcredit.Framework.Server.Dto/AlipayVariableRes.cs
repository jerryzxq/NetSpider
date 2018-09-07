using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.Framework.Server.Dto
{
    public class AlipayVariableRes
    {
        /// <summary>
        /// [申请人]支付宝是否实名
        /// </summary>
        public string IsRealName { get; set; }
        /// <summary>
        /// 账户使用时长（月）
        /// </summary>

        public int Used_Month { get; set; }

        /// <summary>
        /// 绑定银行卡数
        /// </summary>
        public int Bind_Card_Cnt { get; set; }

        /// <summary>
        /// 绑定地址数
        /// </summary>
        public int Bind_Address_Cnt { get; set; }

        /// <summary>
        /// 6个月消费金额
        /// </summary>
        public decimal In6_Consume_Amount { get; set; }

        /// <summary>
        /// 6个月消费笔数
        /// </summary>
        public int In6_Consume_Cnt { get; set; }

        /// <summary>
        /// 蚂蚁花呗总额度
        /// </summary>
        public decimal Flowers_Balance { get; set; }

        /// <summary>
        /// 蚂蚁花呗可用额度
        /// </summary>
        public decimal Flower_Available { get; set; }


    }
}
