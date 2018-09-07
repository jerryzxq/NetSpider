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

    public class AlipayBillExpenditureBll : Business<AlipayBillExpenditureEntity, SqlConnectionFactory>
    {


        public static readonly AlipayBillExpenditureBll Initialize = new AlipayBillExpenditureBll();
        AlipayBillExpenditureBll() { }

        public AlipayBillExpenditureEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayBillExpenditureEntity> List()
        {
            return Select();
        }

        public List<AlipayBillExpenditureEntity> List(SqlExpression<AlipayBillExpenditureEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(AlipayBillExpenditureEntity item)
        {
            return base.Save(item);
        }

        public void ActionSave(AlipayBillEntity bill, AlipayBillExpenditureEntity entity)
        {
            try
            {
                if (entity == null) return;
                entity.BillID = bill.ID;
                base.Save(entity);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillExpenditureBll--ActionSave", ex);

            }
        }


    }
}
