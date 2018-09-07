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

    public class AlipayElectronicBillBll : Business<AlipayElectronicBillEntity, SqlConnectionFactory>
    {


        public static readonly AlipayElectronicBillBll Initialize = new AlipayElectronicBillBll();
        AlipayElectronicBillBll() { }

        public AlipayElectronicBillEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayElectronicBillEntity> List()
        {
            return Select();
        }

        public List<AlipayElectronicBillEntity> List(SqlExpression<AlipayElectronicBillEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(AlipayElectronicBillEntity item)
        {
            return base.Save(item);
        }


        public void ActionSave(AlipayBaicEntity bill, AlipayElectronicBillEntity entity)
        {
            if (entity == null) return;
            entity.BaicID = bill.ID;
            if (base.Save(entity))
            {
                entity.SaveAction<AlipayConsumptionEntity>(entity.Consumption, AlipayConsumptionBll.Initialize.ActionSave);
                entity.SaveAction<AlipayExpBankEntity>(entity.ExpBank, AlipayExpBankBll.Initialize.ActionSave);
                entity.SaveAction<AlipayExpSummaryEntity>(entity.ExpSummary, AlipayExpSummaryBll.Initialize.ActionSave);
                entity.SaveAction<AlipayIncomeSummaryEntity>(entity.IncomeSummary, AlipayIncomeSummaryBll.Initialize.ActionSave);
            }
        }
        public void ActionSave(AlipayBaicEntity bill, List<AlipayElectronicBillEntity> list)
        {
            if (list==null||list.Count == 0) return;
            list.ForEach(e => ActionSave(bill, e));

        }
    }


    public static class AlipayElectronicBillExt
    {

        public static void SaveAction<T>(this AlipayElectronicBillEntity baic, List<T> list, Action<AlipayElectronicBillEntity, List<T>> action)
        {
            try
            {
                if (list == null || list.Count == 0) return;
                action.Invoke(baic, list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayElectronicBillExt", ex);
            }
        }
        public static void SaveAction<T>(this AlipayElectronicBillEntity baic, T list, Action<AlipayElectronicBillEntity, T> action)
        {
            try
            {
                if (list == null) return;
                action.Invoke(baic, list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayElectronicBillExt", ex);
            }
        }

    }
}
