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
using Vcredit.NetSpider.Emall.Dto;
namespace Vcredit.NetSpider.Emall.Business
{
	
	public class SuningOrderBll : Business<SuningOrderEntity, SqlConnectionFactory>
	{
	
	  
        public static readonly SuningOrderBll Initialize = new SuningOrderBll();
        SuningOrderBll() { }

	    public SuningOrderEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<SuningOrderEntity> SelectOrderEntityByAccountName(string accountName)
        {
            var orderList = Select(e => e.AccountName == accountName);
            orderList.ForEach(e => {
                e.OrderGoods = SuningGoodsBll.Initialize.ListByOrderNo(e.ID);
            });
            return orderList;
        }

        public List<SuningOrderEntity> SelectOrderListByUserId(QueryOrderReq queryParams,ref long totalItem)
        {
            var user = SuningUserInfoBll.Initialize.GetUserInfoByToken(queryParams.Token);
            if(user == null) return null;

            SqlExpression<SuningOrderEntity> orderExp = SqlExpression();
            orderExp.Where(e=>e.UserId == user.ID);//根据账户编号过滤

            //根据最新时间过滤
            if (queryParams.LatestTime != null)
                orderExp.Where(e => e.OrderTime >= queryParams.LatestTime.Value);

            totalItem = this.Count(orderExp);
            orderExp.OrderByDescending(e => e.OrderTime).Skip((queryParams.PageIndex-1)*queryParams.PageSize).Take(queryParams.PageSize);

            var orderList = this.Select(orderExp);  

            orderList.ForEach(e => 
            {
                e.OrderGoods = SuningGoodsBll.Initialize.ListByOrderNo(e.ID);
                e.OrderLogistics = SuningLogisticsDetailBll.Initialize.ListByOrderId(e.ID);
            });

            return orderList;
        }

        public List<SuningOrderEntity> OrderListByAccountName(string accountName)
        {
            return Select(e=>e.AccountName == accountName);
        }

        public List<SuningOrderEntity> List()
        {
            return Select();
        }

        public List<SuningOrderEntity> List(SqlExpression<SuningOrderEntity> expression)
        {
            return Select(expression);
        }

        public void Save(List<SuningOrderEntity> list)
        {
            if (list == null || list.Count == 0) return ; 

            foreach (var item in list)
            {
                try
                {
                    if (base.Save(item))
                    {
                        item.SaveAction<SuningGoodsEntity>(item.OrderGoods, SuningGoodsBll.Initialize.ActionSave);
                        item.SaveAction<SuningLogisticsDetailEntity>(item.OrderLogistics, SuningLogisticsDetailBll.Initialize.ActionSave);
                    }
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError("SuningOrderExt", ex);
                }
            } 
        }

        public override bool Save(SuningOrderEntity item)
        {
            return base.Save(item);
        }


        public string GetLastOrderTimeByAccountName(string accountName)
        {
            SqlExpression<SuningOrderEntity> ex = SqlExpression();
            ex.Where(e => e.AccountName == accountName).OrderByDescending(e => e.OrderTime).Take(1);
            var entiy = this.Single(ex);
            return entiy == null ?"" : entiy.OrderTime.Value.ToString("yyyy-MM-dd");
        }

        public void ActionSave(SuningUserInfoEntity sn, List<SuningOrderEntity> list)
        {
            if (list == null || list.Count == 0) return;
            list.ForEach(e => e.UserId = sn.ID);

            foreach (var item in list)
            {
                try
                {
                    if (base.Save(item))
                    {
                        item.SaveAction<SuningGoodsEntity>(item.OrderGoods, SuningGoodsBll.Initialize.ActionSave);
                    }
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError("SuningOrderExt", ex);
                }
            }
        }
	}

    public static class SuningOrderExt
    {
        public static void SaveAction<T>(this SuningOrderEntity baic, List<T> list, Action<SuningOrderEntity, List<T>> action)
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
        public static void SaveAction<T>(this SuningOrderEntity baic, T list, Action<SuningOrderEntity, T> action)
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
