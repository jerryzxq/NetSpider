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
	
	public class SuningUserInfoBll : Business<SuningUserInfoEntity, SqlConnectionFactory>
	{
	
	  
        public static readonly SuningUserInfoBll Initialize = new SuningUserInfoBll();
        SuningUserInfoBll() { }

	    public SuningUserInfoEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<SuningUserInfoEntity> List()
        {
            return Select();
        }

        public List<SuningUserInfoEntity> List(SqlExpression<SuningUserInfoEntity> expression)
        {
            return Select(expression);
        }

        public SuningUserInfoEntity GetUserInfoByToken(string token)
        {
            return Single(e => e.Token == token);
        }

        public SuningUserInfoEntity GetUserInfoEntityByAccountName(string token)
        {
            return Single(e => e.Token == token);
        }

        //public override bool Save(SuningUserInfoEntity item)
        //{
        //    return base.Save(item);
        //}


        public long Insert(SuningUserInfoEntity userInfo)
        {
            return base.InsertIdentity(userInfo); 
        }

        public override bool Save(SuningUserInfoEntity item)
        {

            if (base.Save(item))
            {
                item.SaveAction<SuningReceiveAddressEntity>(item.Address, SuningReceiveAddressBll.Initialize.ActionSave);
                item.SaveAction<SuningCampusAccountEntity>(item.Campus, SuningCampusAccountBll.Initialize.ActionSave);
                item.SaveAction<SuningOrderEntity>(item.Order, SuningOrderBll.Initialize.ActionSave);
                item.SaveAction<SuningCloudVipOrderEntity>(item.CloudVipOrder, SuningCloudVipOrderBll.Initialize.ActionSave);
                item.SaveAction<SuningCartEntity>(item.Cart, SuningCartBll.Initialize.ActionSave);
                item.SaveAction<SuningProductCollectEntity>(item.ProductCollect, SuningProductCollectBll.Initialize.ActionSave);
                item.SaveAction<SuningFootPrintEntity>(item.FootPrintList, SuningFootPrintBll.Initialize.ActionSave);
                item.SaveAction<SuningReturnGoodsOrderEntity>(item.ReturnGoodsList, SuningReturnGoodsOrderBll.Initialize.ActionSave);
                item.SaveAction<SuningEFuBaoBaicEntity>(item.EFuBaoBaic, SuningEFuBaoBaicBll.Initialize.ActionSave);
                item.SaveAction<SuningEFuBaoTradeEntity>(item.EFuBaoTrade, SuningEFuBaoTradeBll.Initialize.ActionSave);
                item.SaveAction<SuningEFuBaoBankCardEntity>(item.EFuBaoBank, SuningEFuBaoBankCardBll.Initialize.ActionSave);
                item.SaveAction<SuningEFuBaoBillPayRecordEntity>(item.BillPayRecord, SuningEFuBaoBillPayRecordBll.Initialize.ActionSave);
                return true;
            }
            return false;
        }

	}

    public static class SuningUserInfoBllExt
    {
        public static void SaveAction<T>(this SuningUserInfoEntity baic, List<T> list, Action<SuningUserInfoEntity, List<T>> action)
        {
            if (list.Count == 0) return;
            action.Invoke(baic, list);
        }
        public static void SaveAction<T>(this SuningUserInfoEntity baic, T list, Action<SuningUserInfoEntity, T> action)
        {
            if (list == null) return;
            action.Invoke(baic, list);
        }

    }
}
