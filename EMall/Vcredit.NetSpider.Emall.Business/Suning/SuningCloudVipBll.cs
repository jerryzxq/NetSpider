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
	
	public class SuningCloudVipOrderBll : Business<SuningCloudVipOrderEntity, SqlConnectionFactory>
	{


        public static readonly SuningCloudVipOrderBll Initialize = new SuningCloudVipOrderBll();
        SuningCloudVipOrderBll() { }

        public SuningCloudVipOrderEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<SuningCloudVipOrderEntity> List()
        {
            return Select();
        }

        public List<SuningCloudVipOrderEntity> List(SqlExpression<SuningCloudVipOrderEntity> expression)
        {
            return Select(expression);
        }

        public override void Insert(List<SuningCloudVipOrderEntity> list)
        {
            base.Insert(list);
        }

        public override bool Save(SuningCloudVipOrderEntity item)
        {
            return base.Save(item);
        }

        public void ActionSave(SuningUserInfoEntity baic, List<SuningCloudVipOrderEntity> list)
        {

            if (list == null || list.Count == 0) return;
            list.ForEach(e => e.UserId = baic.ID);
            base.Save(list);
        }

        public DateTime GetLastCloudVipByAccountName(string accountName)
        {
            SqlExpression<SuningCloudVipOrderEntity> ex = SqlExpression();
            ex.Where(e => e.AccountName == accountName).OrderByDescending(e => e.CloudVipCreateTime).Take(1);
            var entiy = this.Single(ex);
            return entiy != null ? entiy.CloudVipCreateTime.Value : DateTime.Parse("2000-01-01");
        }
	}
}
