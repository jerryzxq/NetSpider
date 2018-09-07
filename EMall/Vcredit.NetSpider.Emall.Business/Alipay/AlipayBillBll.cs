using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
namespace Vcredit.NetSpider.Emall.Business
{

    public class AlipayBillBll : Business<AlipayBillEntity, SqlConnectionFactory>
    {


        public static readonly AlipayBillBll Initialize = new AlipayBillBll();
        AlipayBillBll() { }

        public AlipayBillEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayBillEntity> List()
        {
            return Select();
        }

        public List<AlipayBillEntity> List(SqlExpression<AlipayBillEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(AlipayBillEntity item)
        {
            return base.Save(item);
        }

        public void ActionSave(AlipayBaicEntity baic, List<AlipayBillEntity> list)
        {
            if (list.Count == 0) return;
            list.ForEach(e =>
            {
                e.BaicID = baic.ID;
                e.AccountName = baic.UserID;
              
            });
            foreach (var item in list)
            {
                try
                {
                    if (Save(item))
                    {
                        item.SaveAction<AlipayBillCreditPaymentEntity>(item.BillCreditPayment, AlipayBillCreditPaymentBll.Initialize.ActionSave);
                        item.SaveAction<AlipayBillExpenditureEntity>(item.BillExpenditure, AlipayBillExpenditureBll.Initialize.ActionSave);
                        item.SaveAction<AlipayBillPaymentEntity>(item.BillPayment, AlipayBillPaymentBll.Initialize.ActionSave);
                        item.SaveAction<AlipayBillRefundsEntity>(item.BillRefunds, AlipayBillRefundsBll.Initialize.ActionSave);
                        item.SaveAction<AlipayBillTransferEntity>(item.BillTransfer, AlipayBillTransferBll.Initialize.ActionSave);
                        item.SaveAction<AlipayBillOrderEntity>(item.BillOrder, AlipayBillOrderBll.Initialize.ActionSave);
                    }
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError("AlipayElectronicBillExt", ex);
                }
            }
        }

        public DateTime GetLastBillTimeByAccountName(string accountName)
        {
            SqlExpression<AlipayBillEntity> ex = SqlExpression();
            ex.Where(e => e.AccountName == accountName).OrderByDescending(e => e.BillTime).Take(1);
            var entiy = this.Single(ex);
            return entiy != null ? entiy.BillTime.Value : DateTime.Parse("2000/05/01");
        }
        public DateTime GetLastBillTimeByUserID(string userid,string tradetype)
        {
            SqlExpression<AlipayBillEntity> ex = SqlExpression();
            ex.Where(e => e.AccountName == userid && e.AccountName != "" && e.TradeType== tradetype).OrderByDescending(e => e.BillTime).Take(1);
            var entiy = this.Single(ex);
            return entiy != null ? entiy.BillTime.Value : DateTime.Parse("2000/05/01");
        }
    }



    public static class AlipayBillExt
    {

        public static void SaveAction<T>(this AlipayBillEntity baic, List<T> list, Action<AlipayBillEntity, List<T>> action)
        {
            try
            {
                if (list == null || list.Count == 0) return;
                action.Invoke(baic, list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillExt", ex);
            }
        }
        public static void SaveAction<T>(this AlipayBillEntity baic, T list, Action<AlipayBillEntity, T> action)
        {
            try
            {
                if (list == null) return;
                action.Invoke(baic, list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillExt", ex);
            }
        }

    }
}
