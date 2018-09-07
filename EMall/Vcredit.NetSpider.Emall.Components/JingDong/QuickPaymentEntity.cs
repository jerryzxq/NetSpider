using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace   Vcredit.NetSpider.Emall.Entity 
{
    [Alias("JD_QuickPayment")]
    
    public class QuickPaymentEntity : BaseEntity  //快捷支付详情
    {
        /// <summary>
        /// 银行名称
        /// </summary>
  
        public string BankName { get; set; }

        /// <summary>
        /// 银行卡尾号
        /// </summary>
  
        public string BankCardNumber { get; set; }


        /// <summary>
        /// 持卡人姓名
        /// </summary>
   
        public string Name { get; set; }


        /// <summary>
        /// 持卡人手机号
        /// </summary>
      
        public string Phone { get; set; }

        /// <summary>
        /// 银行卡类别（银行卡/信用卡）
        /// </summary>
   
        public string BankCardType { get; set; }

    }
}
