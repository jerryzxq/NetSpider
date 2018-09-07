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
	
	public class SuningProductCollectBll : Business<SuningProductCollectEntity, SqlConnectionFactory>
	{
        
        public static readonly SuningProductCollectBll Initialize = new SuningProductCollectBll();
        SuningProductCollectBll() { }

        public SuningProductCollectEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<SuningProductCollectEntity> List()
        {
            return Select();
        }

        public List<SuningProductCollectEntity> List(SqlExpression<SuningProductCollectEntity> expression)
        {
            return Select(expression);
        }

        public override void Insert(List<SuningProductCollectEntity> list)
        {
            base.Insert(list);
        }

        public override bool Save(SuningProductCollectEntity item)
        {
            return base.Save(item);
        }


        public DateTime GetLastCollectTimeByAccountName(string accountName)
        {
            SqlExpression<SuningProductCollectEntity> ex = SqlExpression();
            ex.Where(e => e.AccountName == accountName).OrderByDescending(e => e.CollectDateTime).Take(1);
            var entiy = this.Single(ex);
            return entiy == null ? Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd")) : entiy.CollectDateTime.Value;
        }
        public void ActionSave(SuningProductCollectEntity snOrder, List<SuningProductCollectEntity> snOrderGoods)
        {
             
            base.Save(snOrderGoods);
        }

        public void ActionSave(SuningUserInfoEntity baic, List<SuningProductCollectEntity> list)
        {

            if (list == null || list.Count == 0) return;
            list.ForEach(e => e.UserId = baic.ID);
            base.Save(list);
        }
	}
}
