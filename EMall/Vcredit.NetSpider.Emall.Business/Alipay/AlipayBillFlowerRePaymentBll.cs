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

    public class AlipayBillFlowerRePaymentBll : Business<AlipayBillFlowerRePaymentEntity, SqlConnectionFactory>
    {


        public static readonly AlipayBillFlowerRePaymentBll Initialize = new AlipayBillFlowerRePaymentBll();
        AlipayBillFlowerRePaymentBll() { }

        public AlipayBillFlowerRePaymentEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayBillFlowerRePaymentEntity> List()
        {
            return Select();
        }

        public List<AlipayBillFlowerRePaymentEntity> List(SqlExpression<AlipayBillFlowerRePaymentEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(AlipayBillFlowerRePaymentEntity item)
        {
            return base.Save(item);
        }



        public void ActionSave(AlipayBillFlowerEntity billFlower, List<AlipayBillFlowerRePaymentEntity> list)
        {
            try
            {
                if (list == null || list.Count == 0) return;
                list.ForEach(e => e.BillFlowerID = billFlower.ID);
                base.Save(list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillFlowerRePaymentBll--ActionSave", ex);

            }
        }

        public void ActionDeleteById(AlipayBillFlowerEntity billFlower, List<AlipayBillFlowerRePaymentEntity> list)
        {
            try
            {
                if (list == null || list.Count == 0) return;

                foreach (var item in SelectBillFlowerRePaymentByFlowerId(billFlower.ID))
                {
                    base.DeleteById(item.ID);
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("ActionDeleteById", ex);

            }
        }

        private List<AlipayBillFlowerRePaymentEntity> SelectBillFlowerRePaymentByFlowerId(long billFlowerId)
        {
            SqlExpression<AlipayBillFlowerRePaymentEntity> ex = SqlExpression();
            //对最近3个月的数据进行分析
            ex.Where(e => e.BillFlowerID == billFlowerId);
            return this.Select(ex).ToList();
        }
    }
}
