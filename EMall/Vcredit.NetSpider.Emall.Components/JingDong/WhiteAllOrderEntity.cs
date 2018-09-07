using Newtonsoft.Json;
using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Common.Utility;

namespace Vcredit.NetSpider.Emall.Entity
{
    public  class InstallmentPlan
    {
        public string baseAmount { get; set; }
        public string stagesStatus { get; set; }
        public string termNo { get; set; }
        public string totalInterest { get; set; }
        public string totalFeeAmount { get; set; }

        public string totalAmount { get; set; }
        public string refundDate { get; set; }
    }
    [Alias("JD_WhiteAllOrder")]

    /// <summary>
    /// 白条全部订单
    /// </summary>
    public class WhiteAllOrderEntity : BaseEntity
    {
        /// <summary>
        /// 补充分期年份
        /// </summary>
        public void AddYearForInstallment()
        {
            if (installmentPlanList == null && installmentPlanList.Count == 0)
                return;
            try
            {
                var dateArray = ConsumingTime;
                int year = ConsumingTime.Value.Year;
                int month = ConsumingTime.Value.Month;

                foreach (var item in installmentPlanList)
                {
                    var instMonth = int.Parse(item.refundDate.Substring(0, 2));
                    if(instMonth<month)
                    {
                        year += 1;
                        item.refundDate = year.ToString() + "-" + item.refundDate;
                    }
                    else
                    {
                        item.refundDate = year.ToString() + "-" + item.refundDate;
                    }
                    month = instMonth;

                }
            }
            catch(Exception  ex)
            {
                Log4netAdapter.WriteError(OrderNo+"分期年份赋值是把你", ex);

            }


        }
        [Ignore]
        public List<InstallmentPlan> installmentPlanList { get; set; }
        [JsonProperty(PropertyName = "jdOrderNo", NullValueHandling = NullValueHandling.Ignore)]
        public string OrderNo { get; set; }
        [JsonProperty(PropertyName = "originalTerms", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// 期数
        /// </summary>
        public string NumberOfPeriods { get; set; }

        [JsonProperty(PropertyName = "orderSlug", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// 商品名称
        /// </summary>      
        public string CommodityName { get; set; }
        [JsonProperty(PropertyName = "orderAmount", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// 消费金额
        /// </summary>
        public decimal ConsumptionAmount { get; set; }
        [JsonProperty(PropertyName = "retailDate", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// 消费时间
        /// </summary>
        public DateTime? ConsumingTime { get; set; }
        /// <summary>
        /// 应还明细
        /// </summary>
        public string ShouldPayDetail { get; set; }
        /// <summary>
        /// 应还款时间
        /// </summary>
        public DateTime? RepaymentTime { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public string OrderStatus { get; set; }
        /// <summary>
        /// 剩余待还款期数
        /// </summary>
        public int? LeftPeriods { get; set; }
        [JsonProperty(PropertyName = "obligationAmount", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// 剩余待还款总额
        /// </summary>
        public decimal? LeftAmount { get; set; }
        [JsonProperty(PropertyName = "totalInterest", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// 违约金
        /// </summary>
        public decimal DamageFee { get; set; }
        [Ignore]
        /// <summary>
        /// 是否逾期
        /// </summary>
        public bool IsOverDue { get; set; }


    }
}
