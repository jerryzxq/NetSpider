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
	
	public class SuningEFuBaoBillPayRecordBll : Business<SuningEFuBaoBillPayRecordEntity, SqlConnectionFactory>
	{
        public static readonly SuningEFuBaoBillPayRecordBll Initialize = new SuningEFuBaoBillPayRecordBll();
        SuningEFuBaoBillPayRecordBll() { }

	    public SuningEFuBaoBillPayRecordEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<SuningEFuBaoBillPayRecordEntity> List()
        {
            return Select();
        }

        public List<SuningEFuBaoBillPayRecordEntity> List(SqlExpression<SuningEFuBaoBillPayRecordEntity> expression)
        {
            return Select(expression);
        }

        public List<SuningEFuBaoBillPayRecordEntity> GetSuningEFuBaoPayByToken(QueryOrderReq queryParams,ref long totalItem)
        {
            var user = SuningUserInfoBll.Initialize.GetUserInfoByToken(queryParams.Token);
            if (user == null) return null;

            SqlExpression<SuningEFuBaoBillPayRecordEntity> payExp = SqlExpression();

            payExp.Where(e=>e.UserId == user.ID);
            if (queryParams.LatestTime != null)
                payExp.Where(e => e.BillTime >= queryParams.LatestTime.Value);

            totalItem = this.Count(payExp);
            payExp.OrderByDescending(e => e.BillTime).Skip((queryParams.PageIndex - 1) * queryParams.PageSize).Take(queryParams.PageSize);
             
            return Select(payExp); 
        }

        public override void Insert(List<SuningEFuBaoBillPayRecordEntity> list)
        {
            base.Insert(list);
        }


        public DateTime GetLastTradeTimeByAccountName(string accountName)
        {
            SqlExpression<SuningEFuBaoBillPayRecordEntity> ex = SqlExpression();
            ex.Where(e => e.AccountName == accountName).OrderByDescending(e => e.BillTime).Take(1);
            var entiy = this.Single(ex);
            return entiy == null ? Convert.ToDateTime("2011-01-01") : entiy.BillTime.Value.AddDays(1);
        }

        public void ActionSave(SuningUserInfoEntity baic, List<SuningEFuBaoBillPayRecordEntity> list)
        {
            if (list.Count == 0) return;
            list.ForEach(e => e.UserId = baic.ID);
            base.Save(list);
        }
        //public void SaveAsync(SuningEfubaobillpayrecordEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}
	}
}
