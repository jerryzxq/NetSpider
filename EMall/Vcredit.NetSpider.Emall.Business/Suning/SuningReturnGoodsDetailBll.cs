using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity;
using System.Threading.Tasks;
namespace Vcredit.NetSpider.Emall.Business
{

    public class SuningReturnGoodsDetailBll : Business<SuningReturnGoodsDetailEntity, SqlConnectionFactory>
	{
	
	  
        public static readonly SuningReturnGoodsDetailBll Initialize = new SuningReturnGoodsDetailBll();
        SuningReturnGoodsDetailBll() { }

        public SuningReturnGoodsDetailEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<SuningReturnGoodsDetailEntity> List()
        {
            return Select();
        }

        public List<SuningReturnGoodsDetailEntity> List(SqlExpression<SuningReturnGoodsDetailEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(SuningReturnGoodsDetailEntity item)
        {
            return base.Save(item);
        }

       
        public void ActionSave(SuningReturnGoodsOrderEntity order, SuningReturnGoodsDetailEntity detail)
        {
            if (string.IsNullOrWhiteSpace(detail.ServiceType) && string.IsNullOrWhiteSpace(detail.ServiceTypeReason)) return;
            detail.ReturnID = order.ID;
            this.Save(detail);
        }

	}
}
