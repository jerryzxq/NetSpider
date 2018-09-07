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

    public class SuningCampusAccountBll : Business<SuningCampusAccountEntity, SqlConnectionFactory>
	{
	
	  
        public static readonly SuningCampusAccountBll Initialize = new SuningCampusAccountBll();
        SuningCampusAccountBll() { }

        public SuningCampusAccountEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<SuningCampusAccountEntity> List()
        {
            return Select();
        }

        public List<SuningCampusAccountEntity> List(SqlExpression<SuningCampusAccountEntity> expression)
        {
            return Select(expression);
        }

        public override bool Insert(SuningCampusAccountEntity item)
        {
            return base.Insert(item);
        }

        public override bool Save(SuningCampusAccountEntity item)
        {
            return base.Save(item);
        }

       
        public void ActionSave(SuningUserInfoEntity baic, SuningCampusAccountEntity item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.RealName)) return;
            item.BaicID = baic.ID;
            base.Save(item);
        }

	}
}
