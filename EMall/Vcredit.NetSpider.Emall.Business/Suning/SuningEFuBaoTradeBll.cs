using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite; 
using Vcredit.NetSpider.Emall.Entity;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Emall.Dto;
namespace Vcredit.NetSpider.Emall.Business
{
	
	public class SuningEFuBaoTradeBll : Business<SuningEFuBaoTradeEntity, SqlConnectionFactory>
	{
        public static readonly SuningEFuBaoTradeBll Initialize = new SuningEFuBaoTradeBll();
        SuningEFuBaoTradeBll() { }

        public SuningEFuBaoTradeEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<SuningEFuBaoTradeEntity> List()
        {
            return Select();
        }

        public List<SuningEFuBaoTradeEntity> List(SqlExpression<SuningEFuBaoTradeEntity> expression)
        {
            return Select(expression);
        }
        public DateTime GetLastTradeTimeByAccountName(string accountName)
        {
            SqlExpression<SuningEFuBaoTradeEntity> ex = SqlExpression();
            ex.Where(e => e.AccountName == accountName).OrderByDescending(e => e.TradeTime).Take(1);
            var entiy = this.Single(ex);
            return entiy == null ? Convert.ToDateTime("2011-01-01") : entiy.TradeTime.Value;
        }

        public List<SuningEFuBaoTradeEntity> GetSuningEFuBaoTradeByToken(QueryOrderReq queryParams,ref long totalItem)
        {
            var user = SuningUserInfoBll.Initialize.GetUserInfoByToken(queryParams.Token);
            if (user == null) return null;

            SqlExpression<SuningEFuBaoTradeEntity> tradeExp = SqlExpression();
            tradeExp.Where(e=>e.UserId == user.ID);

            if (queryParams.LatestTime != null)
                tradeExp.Where(e => e.TradeTime >= queryParams.LatestTime.Value);

            totalItem = this.Count(tradeExp);
            tradeExp.OrderByDescending(e=>e.TradeTime).Skip((queryParams.PageIndex-1)*queryParams.PageSize).Take(queryParams.PageSize);
             
            var list = Select(tradeExp);
            list.ForEach(e => 
            {
                e.TradeProcess = SuningEFuBaoTradeProcessBll.Initialize.SelectListByTradeId(e.ID);
            });

            return list;
        }

        public override void Insert(List<SuningEFuBaoTradeEntity> list)
        {
            if (list == null || list.Count == 0) return; 

            foreach (var item in list)
            {
                try
                {
                    if (base.Save(item))
                    {
                        item.SaveAction<SuningEFuBaoTradeProcessEntity>(item.TradeProcess, SuningEFuBaoTradeProcessBll.Initialize.ActionSave);
                    }
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError("SuningEFuBaoTrade", ex);
                }
            }
            base.Save(list);
        }

        public void ActionSave(SuningUserInfoEntity baic, List<SuningEFuBaoTradeEntity> list)
        {
            if (list == null || list.Count == 0) return;
            list.ForEach(e=>e.UserId = baic.ID);

            foreach (var item in list)
            {
                try
                {
                    if (base.Save(item))
                    {
                        item.SaveAction<SuningEFuBaoTradeProcessEntity>(item.TradeProcess, SuningEFuBaoTradeProcessBll.Initialize.ActionSave);
                    }
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError("SuningOrderExt", ex);
                }
            }
            //base.Save(list);
        }
        //public void SaveAsync(SuningEfubaotradeEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}
	}

    public static class SuningEFuBaoTradeExt
    {
        public static void SaveAction<T>(this SuningEFuBaoTradeEntity baic, List<T> list, Action<SuningEFuBaoTradeEntity, List<T>> action)
        {
            try
            {
                if (list.Count == 0) return;
                action.Invoke(baic, list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("SuningEFuBaoTradeExt", ex);
            }
        }
        public static void SaveAction<T>(this SuningEFuBaoTradeEntity baic, T list, Action<SuningEFuBaoTradeEntity, T> action)
        {
            try
            {
                if (list == null) return;
                action.Invoke(baic, list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("SuningEFuBaoTradeExt", ex);
            }

        }

    }
}
