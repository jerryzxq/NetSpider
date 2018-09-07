using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.NetSpider.Emall.Business.JingDong;
using Vcredit.NetSpider.Emall.Dto.Share;
using Vcredit.NetSpider.Emall.Entity;

namespace Vcredit.NetSpider.Emall.Business.Share.MemberUsers
{
    public class BillSetting
    {
        /// <summary>
        /// 支付宝
        /// </summary>
        /// <param name="alipay"></param>
        /// <returns></returns>
        public BaicBill DoSetting(AlipayBaicEntity alipay)
        {
            BaicBill baic = new BaicBill()
            {
                AvailableBalance = alipay.FlowerAvailable,
                ConsumptionQuota = alipay.FlowersBalance,
                IdentityCard=alipay.DocumentNo,
                Name=alipay.RealName,
                AccountName = alipay.AccountName
            };

            List<DetailBills> list = new List<DetailBills>();
            var flower = AlipayBillFlowerBll.Initialize.Select(e => e.AccountName == alipay.AccountName);
            foreach (var item in flower)
            {
                DetailBills detail = new DetailBills();
                detail.BillAmount = item.BillAmount;
                detail.FinalRepayDate = item.FinalRepayDate;
                detail.HasAmount = item.HasAmount;
                detail.NoHasAmount = item.NoHasAmount;
                detail.OrderStatus = item.OrderStatus;
                detail.OverdueInterest = item.OverdueInterest;
                baic.List.Add(detail);
            }

            return baic;
        }
        /// <summary>
        /// 京东
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public BaicBill DoSetting(UserInfoEntity user)
        {

            JD_JDWhiteBarBll bar = new JD_JDWhiteBarBll();
            JDWhiteBarEntity userbar = bar.Single(e => e.UserId == user.ID);
            BaicBill baic = new BaicBill()
            {
                AvailableBalance = userbar.TotalCreditNum - userbar.HaveUsedNum,
                ConsumptionQuota = userbar.TotalCreditNum,
                IdentityCard=user.IdentityCard,
                Name=user.TrueName,
                AccountName=user.AccountName
            };
            var flower = new JD_WhiteAllOrderBll().Select(e => e.UserId == user.ID);
            JD_WhiteRepayMentBll pepay = new JD_WhiteRepayMentBll();
            foreach (var item in flower)
            {
                var paylist = pepay.Select(e => e.OrderNo == item.OrderNo && e.UserId == user.ID);

                DetailBills detail = new DetailBills();
                detail.BillAmount = item.ConsumptionAmount;
                detail.FinalRepayDate = item.RepaymentTime;
                detail.HasAmount = paylist.Sum(e => e.RepayAmount);
                detail.NoHasAmount = item.ConsumptionAmount - detail.HasAmount;
                detail.OrderStatus = item.OrderStatus;
                detail.OverdueInterest = paylist.Sum(e => e.PayDamageAmount);
                baic.List.Add(detail);
            }

            return baic;
        }
    }
}
