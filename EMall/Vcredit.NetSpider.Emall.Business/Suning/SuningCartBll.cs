using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity;
using System.Threading.Tasks;
using Vcredit.NetSpider.Emall.Dto;
namespace Vcredit.NetSpider.Emall.Business
{
	
	public class SuningCartBll : Business<SuningCartEntity, SqlConnectionFactory>
	{

        public static readonly SuningCartBll Initialize = new SuningCartBll();
        SuningCartBll() { }

        public SuningCartEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<SuningCartEntity> List()
        {
            return Select();
        }

        public List<SuningCartEntity> List(SqlExpression<SuningCartEntity> expression)
        {
            return Select(expression);
        }

        public override void Insert(List<SuningCartEntity> list)
        {
             base.Insert(list); 
        }
        public override bool Save(SuningCartEntity item)
        {
            return base.Save(item);
        }


        public void ActionSave(SuningUserInfoEntity baic, List<SuningCartEntity> list)
        {

            if (list == null || list.Count == 0) return;
            list.ForEach(e => e.UserId = baic.ID);
            base.Save(list);
        }

        public List<SuningCartEntity> SelectListByToken(QueryOrderReq queryParams,ref long totalItem)
        {
            var user = SuningUserInfoBll.Initialize.GetUserInfoByToken(queryParams.Token);
            if (user == null) return null;

            SqlExpression<SuningCartEntity> cartExp = SqlExpression();

            cartExp.Where(e=>e.UserId == user.ID);
            totalItem = this.Count(cartExp);

            cartExp.OrderByDescending(e => e.ID).Skip((queryParams.PageIndex - 1) * queryParams.PageSize).Take(queryParams.PageSize);

            return this.Select(cartExp);
        }
    }
}
