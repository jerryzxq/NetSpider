using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.TaoBao;
using System.Threading.Tasks;
using Vcredit.NetSpider.Emall.Entity;
namespace Vcredit.NetSpider.Emall.Business
{
	
	public class SuningFootPrintBll : Business<SuningFootPrintEntity, SqlConnectionFactory>
	{
        public static readonly SuningFootPrintBll Initialize = new SuningFootPrintBll();
       SuningFootPrintBll() { }

	    public SuningFootPrintEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<SuningFootPrintEntity> List()
        {
            return Select();
        }

        public List<SuningFootPrintEntity> List(SqlExpression<SuningFootPrintEntity> expression)
        {
            return Select(expression);
        }

        public override void Insert(List<SuningFootPrintEntity> list)
        {
            base.Insert(list);
        }

        public override bool Save(SuningFootPrintEntity item)
        {
            return base.Save(item);
        }


        public void ActionSave(SuningUserInfoEntity baic, List<SuningFootPrintEntity> footList)
        {
            if (footList == null || footList.Count == 0) return;

            footList.ForEach(e => e.UserId = baic.ID);
            base.Save(footList);
        }
	}
}
