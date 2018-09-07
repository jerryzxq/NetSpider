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

    public class AlipayBillFlowerBll : Business<AlipayBillFlowerEntity, SqlConnectionFactory>
    {
        public static readonly AlipayBillFlowerBll Initialize = new AlipayBillFlowerBll();
        AlipayBillFlowerBll() { }

        public AlipayBillFlowerEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayBillFlowerEntity> List()
        {
            return Select();
        }

        public List<AlipayBillFlowerEntity> List(SqlExpression<AlipayBillFlowerEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(AlipayBillFlowerEntity item)
        {
            return base.Save(item);
        }



        public void ActionSave(AlipayBaicEntity baic, List<AlipayBillFlowerEntity> list)
        {
            if (list.Count == 0) return;
            list.ForEach(e =>
            {
                e.BaicID = baic.ID;
                e.UpdateTime = DateTime.Now;
                e.AccountName = baic.UserID;
            });

            List<AlipayBillFlowerEntity> oldList = GetBillFlowerListBy(baic.AccountName, baic.UserID);
            if (oldList.Count > 0)
            {
                List<AlipayBillFlowerEntity> updateList = list.Where(f => oldList.Select(o => o.FlowerMonth).Contains(f.FlowerMonth)).ToList();

                oldList = oldList.Where(f => updateList.Select(u => u.FlowerMonth).Contains(f.FlowerMonth)).ToList();
                oldList.ForEach(e =>
                {
                    var currFlower = updateList.Where(f => e.FlowerMonth.Equals(f.FlowerMonth)).FirstOrDefault();
                    if (currFlower != null)
                    {
                        e.BillAmount = currFlower.BillAmount;
                        e.HasAmount = currFlower.HasAmount;
                        e.OverdueInterest = currFlower.OverdueInterest;
                        e.AmortizationAmount = currFlower.AmortizationAmount;
                        e.NoHasAmount = currFlower.NoHasAmount;
                        e.OrderStatus = currFlower.OrderStatus;
                        e.OrderState = currFlower.OrderState;
                        e.FinalRepayDate = currFlower.FinalRepayDate;
                        e.BillFlowerOrderDetail = currFlower.BillFlowerOrderDetail;
                        e.BillFlowerRePayment = currFlower.BillFlowerRePayment;
                        e.UpdateTime = currFlower.UpdateTime;
                    }
                });

                foreach (var item in oldList)
                {
                    try
                    {
                        if (Update(item))
                        {
                            item.SaveAction<AlipayBillFlowerOrderDetailEntity>(item.BillFlowerOrderDetail, AlipayBillFlowerOrderDetailBll.Initialize.ActionDeleteById);
                            item.SaveAction<AlipayBillFlowerRePaymentEntity>(item.BillFlowerRePayment, AlipayBillFlowerRePaymentBll.Initialize.ActionDeleteById);
                            item.SaveAction<AlipayBillFlowerOrderDetailEntity>(item.BillFlowerOrderDetail, AlipayBillFlowerOrderDetailBll.Initialize.ActionSave);
                            item.SaveAction<AlipayBillFlowerRePaymentEntity>(item.BillFlowerRePayment, AlipayBillFlowerRePaymentBll.Initialize.ActionSave);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log4netAdapter.WriteError("AlipayBillFlowerExtUpdate", ex);
                    }
                }

                list = list.Where(f => !updateList.Select(o => o.FlowerMonth).Contains(f.FlowerMonth)).ToList();
            }

            foreach (var item in list)
            {
                try
                {
                    if (Save(item))
                    {
                        item.SaveAction<AlipayBillFlowerOrderDetailEntity>(item.BillFlowerOrderDetail, AlipayBillFlowerOrderDetailBll.Initialize.ActionSave);
                        item.SaveAction<AlipayBillFlowerRePaymentEntity>(item.BillFlowerRePayment, AlipayBillFlowerRePaymentBll.Initialize.ActionSave);

                    }
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError("AlipayBillFlowerExt", ex);
                }
            }

            base.Save(list);
        }

        public List<AlipayBillFlowerEntity> GetillFlowerIn3Use(string userID, string Accountname)
        {
            SqlExpression<AlipayBillFlowerEntity> ex = SqlExpression();
            ex.Join<AlipayBillFlowerEntity, AlipayBillFlowerOrderDetailEntity>((x, y) => x.ID == y.BillFlowerID);
            ex.Where(e => (e.AccountName == userID || e.AccountName == Accountname) && e.AccountName != "");
            ex.Where<AlipayBillFlowerOrderDetailEntity>(x => x.OrderCreateTime >= DateTime.Now.AddMonths(-3));
            return this.Select(ex);
        }

        private List<AlipayBillFlowerEntity> GetBillFlowerListBy(string userid, string Accountname)
        {
            SqlExpression<AlipayBillFlowerEntity> ex = SqlExpression();
            //对最近3个月的数据进行分析
            ex.Where(e => (e.AccountName == userid || e.AccountName == Accountname) && e.AccountName != "").OrderByDescending(e => e.FlowerMonth);
            return this.Select(ex);
        }

        public string GetLastBillTimeBy(string userid, string Accountname)
        {
            SqlExpression<AlipayBillFlowerEntity> ex = SqlExpression();
            //对最近3个月的数据进行分析
            ex.Where(e => (e.AccountName == userid || e.AccountName == Accountname) && e.AccountName != "").OrderByDescending(e => e.FlowerMonth).Take(3);
            var entiy = this.Select(ex);
            string nextMonth = entiy.Count == 0 ? "2015-04" : entiy.FirstOrDefault().FlowerMonth.Insert(4, "-");
            DateTime currentMonth = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM"));

            //当前月前一个月数据
            var mode = entiy.Where(e => Convert.ToDateTime(e.FlowerMonth.Insert(4, "-")) < currentMonth).OrderByDescending(e => e.FlowerMonth).FirstOrDefault();
            if (mode == null) return nextMonth;

            if (mode.OrderState == 1)
            {
                ////当月前一个月已还清
                //mode = entiy.Where(e => Convert.ToDateTime(e.FlowerMonth.Insert(4, "-")) <= Convert.ToDateTime(nextMonth) && e.OrderState > 1).OrderBy(e => e.FlowerMonth).FirstOrDefault();
                //if (mode == null) return Convert.ToDateTime(mode.FlowerMonth.Insert(4, "-")).AddMonths(1).ToString("yyyy-MM");

                //if (mode.BillAmount == mode.HasAmount || mode.OrderState == 1)
                //{
                //    return Convert.ToDateTime(mode.FlowerMonth.Insert(4, "-")).AddMonths(1).ToString("yyyy-MM");
                //}
                //else
                //{
                //    //return nextMonth;
                //    return Convert.ToDateTime(mode.FlowerMonth.Insert(4, "-")).ToString("yyyy-MM");
                //}
                return Convert.ToDateTime(mode.FlowerMonth.Insert(4, "-")).AddMonths(1).ToString("yyyy-MM");//获取还清月的下个月的所有数据
            }
            else
            {
                return mode.FlowerMonth.Insert(4, "-");
            }
        }

        private AlipayBillFlowerEntity GetLastBillTimeBy(string accountName, string userid, string nextMonth)
        {
            SqlExpression<AlipayBillFlowerEntity> ex = SqlExpression();
            ex.Where(e => (e.AccountName == accountName||e.AccountName==userid) && e.FlowerMonth.Equals(nextMonth));
            return (AlipayBillFlowerEntity)this.Single(ex);
        }

    }
    public static class AlipayBillFlowerExt
    {
        public static void SaveAction<T>(this AlipayBillFlowerEntity baic, List<T> list, Action<AlipayBillFlowerEntity, List<T>> action)
        {
            try
            {
                if ( list == null||list.Count == 0 ) return;
                action.Invoke(baic, list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillFlowerExtList", ex);
            }
        }
        public static void SaveAction<T>(this AlipayBillFlowerEntity baic, T item, Action<AlipayBillFlowerEntity, T> action)
        {
            try
            {
                if (item == null) return;
                action.Invoke(baic, item);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillExtItem", ex);
            }

        }

    }
}
