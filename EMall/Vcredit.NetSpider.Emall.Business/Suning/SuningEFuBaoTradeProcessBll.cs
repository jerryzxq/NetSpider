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
	
	public class SuningEFuBaoTradeProcessBll : Business<SuningEFuBaoTradeProcessEntity, SqlConnectionFactory>
	{
        public static readonly SuningEFuBaoTradeProcessBll Initialize = new SuningEFuBaoTradeProcessBll();
        SuningEFuBaoTradeProcessBll() { }

        public SuningEFuBaoTradeProcessEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<SuningEFuBaoTradeProcessEntity> List()
        {
            return Select();
        }

        public List<SuningEFuBaoTradeProcessEntity> List(SqlExpression<SuningEFuBaoTradeProcessEntity> expression)
        {
            return Select(expression);
        }

        public List<SuningEFuBaoTradeProcessEntity> SelectListByTradeId(long tradeId)
        {
            return Select(e=>e.TradeID == tradeId);
        }

        public void ActionSave(SuningEFuBaoTradeEntity trade, List<SuningEFuBaoTradeProcessEntity> list)
        {

            if (list == null || list.Count == 0) return;
            list.ForEach(e => e.TradeID = trade.ID);
            base.Save(list);
        }
        //public void SaveAsync(SuningEfubaotradeprocessEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}
	}
}
