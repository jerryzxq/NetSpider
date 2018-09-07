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
	
	public class AmazonOrderBll : Business<AmazonOrderEntity, SqlConnectionFactory>
	{
        public static readonly AmazonOrderBll Initialize = new AmazonOrderBll();
        AmazonOrderBll() { }

        public AmazonOrderEntity Load(long id)
        {
            return Single(e => e.ID == id);
        }

        public List<AmazonOrderEntity> List()
        {
            return Select();
        }

        public List<AmazonOrderEntity> SelectOrderEntityByUserId(long userId)
        {
            List<AmazonOrderEntity> list = Select(e => e.UserId == userId);
            list.ForEach(e =>
            { 
                e.GoodsList = AmazonGoodsBll.Initialize.SelectAmazonGoodsListByOrderId(e.ID);
            });
            return list;
        }

        public List<AmazonOrderEntity> List(SqlExpression<AmazonOrderEntity> expression)
        {
            return Select(expression);
        }
        public void ActionSave(List<AmazonOrderEntity> list)
        {
            if (list == null || list.Count == 0) return;
            
            foreach (var item in list)
            {
                try
                {
                    if (base.Save(item))
                    {
                        item.SaveAction<AmazonGoodsEntity>(item.GoodsList, AmazonGoodsBll.Initialize.ActionSave);
                    }
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError("SuningOrderExt", ex);
                }
            }
        }
        //public void SaveAsync(AmazonOrderEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}

        public List<AmazonOrderEntity> SelectOrderListByAccountName(string accountName)
        {
            return Select(e=>e.AccountName == accountName);
        }
    }

    public static class AmazonOrderBllExt
    {
        public static void SaveAction<T>(this AmazonOrderEntity baic, List<T> list, Action<AmazonOrderEntity, List<T>> action)
        {
            try
            {
                if (list.Count == 0) return;
                action.Invoke(baic, list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("SuningOrderExt", ex);
            }
        }
        public static void SaveAction<T>(this AmazonOrderEntity baic, T list, Action<AmazonOrderEntity, T> action)
        {
            try
            {
                if (list == null) return;
                action.Invoke(baic, list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("SuningOrderExt", ex);
            }

        }

    }
}
