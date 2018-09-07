using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
namespace Vcredit.NetSpider.Emall.Business
{

    public class SuningReturnGoodsOrderBll : Business<SuningReturnGoodsOrderEntity, SqlConnectionFactory>
	{
	
	  
        public static readonly SuningReturnGoodsOrderBll Initialize = new SuningReturnGoodsOrderBll();
        SuningReturnGoodsOrderBll() { }

	    public SuningReturnGoodsOrderEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<SuningReturnGoodsOrderEntity> List()
        {
            return Select();
        }

        public List<SuningReturnGoodsOrderEntity> List(SqlExpression<SuningReturnGoodsOrderEntity> expression)
        {
            return Select(expression);
        }

        public string GetLastReturnGoodsOrderTimeByAccountName(string accountName)
        {
            SqlExpression<SuningReturnGoodsOrderEntity> ex = SqlExpression();
            ex.Where(e => e.AccountName == accountName).Where(f => f.ReturnStatus == 1);
            var list = this.List(ex);
            if (!list.Any()) return string.Empty;

            return list.OrderByDescending(e => Convert.ToDateTime(e.OrderTime)).Take(1).FirstOrDefault().OrderTime; 
        }

        public string GetLastReturnGoodsSubmitTimeByAccountName(string accountName)
        {
            SqlExpression<SuningReturnGoodsOrderEntity> ex = SqlExpression();
            ex.Where(e => e.AccountName == accountName).Where(e => e.ReturnStatus == 2);
            var list = this.List(ex);
            if (!list.Any()) return string.Empty;

            return list.OrderByDescending(e => Convert.ToDateTime(e.SubmitTime)).Take(1).FirstOrDefault().SubmitTime; 
        }
        public override bool Save(List<SuningReturnGoodsOrderEntity> list)
        {
            if (list == null || list.Count == 0) return false; 

            foreach (var item in list)
            {
                try
                {
                    if (base.Save(item))
                    {
                        item.SaveAction<SuningReturnGoodsDetailEntity>(item.ReturnDetail, SuningReturnGoodsDetailBll.Initialize.ActionSave);
                        item.SaveAction<SuningReturnGoodProcessEntity>(item.ReturnProcessList, SuningReturnGoodProcessBll.Initialize.ActionSave);
                    }
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError("SuningReturnGoodsOrderBll", ex);
                }
            }
            return true;
        }

        public override bool Save(SuningReturnGoodsOrderEntity item)
        {
            return base.Save(item);
        }


        public void ActionSave(SuningUserInfoEntity baic, List<SuningReturnGoodsOrderEntity> list)
        {

            if (list == null || list.Count == 0) return;
            list.ForEach(e => e.UserId = baic.ID);

            foreach (var item in list)
            {
                try
                {
                    if (base.Save(item))
                    {
                        item.SaveAction<SuningReturnGoodsDetailEntity>(item.ReturnDetail, SuningReturnGoodsDetailBll.Initialize.ActionSave);
                        item.SaveAction<SuningReturnGoodProcessEntity>(item.ReturnProcessList, SuningReturnGoodProcessBll.Initialize.ActionSave);
                    }
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError("SuningReturnGoodsOrderBll", ex);
                }
            }
        }
	}

    public static class SuningReturnGoodsOrderExt
    {
        public static void SaveAction<T>(this SuningReturnGoodsOrderEntity baic, List<T> list, Action<SuningReturnGoodsOrderEntity, List<T>> action)
        {
            try
            {
                if (list.Count == 0) return;
                action.Invoke(baic, list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("SuningReturnGoodsOrderExt", ex);
            }
        }
        public static void SaveAction<T>(this SuningReturnGoodsOrderEntity baic, T list, Action<SuningReturnGoodsOrderEntity, T> action)
        {
            try
            {
                if (list == null) return;
                action.Invoke(baic, list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("SuningReturnGoodsOrderExt", ex);
            }

        }

    }
}
