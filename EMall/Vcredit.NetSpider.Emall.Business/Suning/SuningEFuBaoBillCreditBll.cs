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
	
	public class SuningEFuBaoBillCreditBll : Business<SuningEFuBaoBillCreditEntity, SqlConnectionFactory>
	{
        public static readonly SuningEFuBaoBillCreditBll Initialize = new SuningEFuBaoBillCreditBll();
        SuningEFuBaoBillCreditBll() { }

	    public SuningEFuBaoBillCreditEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<SuningEFuBaoBillCreditEntity> List()
        {
            return Select();
        }

        public List<SuningEFuBaoBillCreditEntity> List(SqlExpression<SuningEFuBaoBillCreditEntity> expression)
        {
            return Select(expression);
        }

        public List<SuningEFuBaoBillCreditEntity> GetSuningEFuBaoCreditByToken(QueryOrderReq queryParams,ref long totalItem)
        {
            var user = SuningUserInfoBll.Initialize.GetUserInfoByToken(queryParams.Token);
            if (user == null) return null;

            SqlExpression<SuningEFuBaoBillCreditEntity> creditExp = SqlExpression();
            creditExp.Where(e=>e.UserId == user.ID);

            if (queryParams.LatestTime != null)
                creditExp.Where(e=>e.TradeTime >= queryParams.LatestTime.Value);

            totalItem = this.Count(creditExp);
            creditExp.OrderByDescending(e => e.TradeTime).Skip((queryParams.PageIndex - 1) * queryParams.PageSize).Take(queryParams.PageSize);

            return Select(creditExp);
        }

        public DateTime GetTradeStartTimeByAccountName(string accountName)
        {
            SqlExpression<SuningEFuBaoBillCreditEntity> ex = SqlExpression();
            ex.Where(e => e.AccountName == accountName).OrderByDescending(e => e.TradeTime).Take(1);
            var entiy = this.Single(ex);
            return entiy == null ? Convert.ToDateTime("2011-01-01") : entiy.TradeTime.Value;
        }

        public override void Insert(System.Collections.Generic.List<Vcredit.NetSpider.Emall.Entity.SuningEFuBaoBillCreditEntity> list)
        {
            if (list == null || list.Count == 0) return;
            base.Insert(list);
        }
	}
}
