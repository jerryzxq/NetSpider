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

    public class AlipayExpBankBll : Business<AlipayExpBankEntity, SqlConnectionFactory>
    {


        public static readonly AlipayExpBankBll Initialize = new AlipayExpBankBll();
        AlipayExpBankBll() { }

        public AlipayExpBankEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayExpBankEntity> List()
        {
            return Select();
        }

        public List<AlipayExpBankEntity> List(SqlExpression<AlipayExpBankEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(AlipayExpBankEntity item)
        {
            return base.Save(item);
        }


        public void ActionSave(AlipayElectronicBillEntity bill, AlipayExpBankEntity entity)
        {
            try
            {
                if (entity == null) return;
                entity.ElectronicBillID = bill.ID;
                base.Save(entity);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayExpBankBll--ActionSave", ex);
            }

        }
        public void ActionSave(AlipayElectronicBillEntity bill, List<AlipayExpBankEntity> list)
        {
            try
            {
                if (list == null || list.Count == 0) return;
                list.ForEach(e => e.ElectronicBillID = bill.ID);
                base.Save(list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayExpBankBll--ActionSave", ex);
            }
        }
    }
}
