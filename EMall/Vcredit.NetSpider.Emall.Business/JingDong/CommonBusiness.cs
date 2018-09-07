using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Emall.Entity;
using Vcredit.Common.Utility;
using System.Threading.Tasks;

namespace Vcredit.NetSpider.Emall.Business.JingDong
{


    public class CommonBusiness
    {

        readonly JD_BrowseHistorieBll browseBus = new JD_BrowseHistorieBll();
        readonly JD_ExpenseRecordBll expBus = new JD_ExpenseRecordBll();
        readonly JD_FocusActivityBll focaBus = new JD_FocusActivityBll();
        readonly JD_FocusBrandBll focbBus = new JD_FocusBrandBll();
        readonly JD_FocusProductBll focpBus = new JD_FocusProductBll();
        readonly JD_FocusShopBll focShBus = new JD_FocusShopBll();
        readonly JD_GoodsBll goodsBus = new JD_GoodsBll();
        readonly JD_GrowthValueDetailBll growBus = new JD_GrowthValueDetailBll();
        readonly JD_OrderDetailBll orderBus = new JD_OrderDetailBll();
        readonly JD_QuickPaymentBll quickBus = new JD_QuickPaymentBll();
        readonly JD_ReceiveAddresseBll recBus = new JD_ReceiveAddresseBll();
        readonly JD_ShareDetailBll shareBus = new JD_ShareDetailBll();
        readonly JD_WhiteAllOrderBll wallorder = new JD_WhiteAllOrderBll();
        readonly JD_WhiteNeedPayBll wNeedpay = new JD_WhiteNeedPayBll();
        readonly JD_WhiteRefundedBll wrefunded = new JD_WhiteRefundedBll();
        readonly JD_WhiteRepayMentBll wrepay = new JD_WhiteRepayMentBll();
        readonly JD_ShoppingCartBll shoppingcart = new JD_ShoppingCartBll();
        readonly JD_LogisticsBll logisticsbll = new JD_LogisticsBll();
        readonly JD_InstalmentBll ins = new JD_InstalmentBll();
        public void InsertJDInfo(JDInfosEntity jdInfo)
        {
            int userid = jdInfo.UserInfo.ID;
            if (userid <= 0)
            {
                throw new Exception(jdInfo.UserInfo.AccountName + "用户信息入库失败。");
            }
            actionTryInsert(jdInfo.ExpenseRecord, userid, (param) => expBus.Insert(param));
            actionTryInsert(jdInfo.WhiteAllOrderList, userid, (param) => wallorder.Insert(param));
            actionTryInsert(jdInfo.WhiteNeedPayList, userid, (param) => wNeedpay.Insert(param));
            actionTryInsert(jdInfo.WhiteRefundedList, userid, (param) => wrefunded.Insert(param));
            actionTryInsert(jdInfo.WhiteRepayMentList, userid, (param) => wrepay.Insert(param));
            actionTryInsert(jdInfo.InstalmentList, userid, (param) => ins.Insert(param));
            actionTryInsert(jdInfo.BrowseHistory, userid, (param) => browseBus.Insert(param));
            actionTryInsert(jdInfo.FocusActivityList, userid, (param) => focaBus.Insert(param));
            actionTryInsert(jdInfo.FocusBrandList, userid, (param) => focbBus.Insert(param));
            actionTryInsert(jdInfo.FocusProductList, userid, (param) => focpBus.Insert(param));
            actionTryInsert(jdInfo.FocusShopList, userid, (param) => focShBus.Insert(param));
            actionTryInsert(jdInfo.GrowthValueDetail, userid, (param) => growBus.Insert(param));
            actionTryInsert(jdInfo.QuickPayment, userid, (param) => quickBus.Insert(param));
            actionTryInsert(jdInfo.ReceiveAddress, userid, (param) => recBus.Insert(param));
            actionTryInsert(jdInfo.ShareList, userid, (param) => shareBus.Insert(param));

        }
        public void InsertShoppingCartGoods(JDInfosEntity jdInfo)
        {
            if (jdInfo.UserInfo.ID == 0)
            {
                Log4netAdapter.WriteInfo(jdInfo.UserInfo.AccountName + "信息入库异常，无法插入订单数据");
                return;
            }
            actionTryInsert(jdInfo.ShppingCartGoodsList, jdInfo.UserInfo.ID, (param) => shoppingcart.Insert(param));
        }

        public void InsertOrderListWithUserID(JDInfosEntity jdInfo)
        {
            if (jdInfo.UserInfo.ID == 0)
            {
                Log4netAdapter.WriteInfo(jdInfo.UserInfo.AccountName+"信息入库异常，无法插入订单数据");
                return;
            }
            List<GoodsEntity> goodsList = new List<GoodsEntity>();
            List<LogisticsEntity> logisticsList = new List<LogisticsEntity>();
            if (orderBus.Exist(x => x.AccountName == jdInfo.UserInfo.AccountName))//如果表里存在数据的过滤掉已经抓取的数据
                jdInfo.OrderList = orderBus.FilterExistOrders(jdInfo.OrderList, jdInfo.UserInfo.AccountName);
            jdInfo.OrderList.ForEach(x => {
                x.AccountName = jdInfo.UserInfo.AccountName;
                x.GoodsList.ForEach(item =>item.AccountName = jdInfo.UserInfo.AccountName);
                goodsList.AddRange(x.GoodsList);

                x.Logistics.AccountName = jdInfo.UserInfo.AccountName;
                x.Logistics.OrderNo = x.OrderNo;
                logisticsList.Add(x.Logistics);   
            });
            actionTryInsert(jdInfo.OrderList, jdInfo.UserInfo.ID, (param) => orderBus.Insert(param));//订单入库
            actionTryInsert(goodsList, jdInfo.UserInfo.ID, (param) => goodsBus.Insert(param));//订单商品入库
            actionTryInsert(logisticsList, jdInfo.UserInfo.ID, (param) => logisticsbll.Insert(param));//订单物流

        }
        private bool  actionTryInsert<T>(List<T> infoList, int userid, Action<List<T>> insertaction) where T : BaseEntity
        {
            try
            {
                if (infoList.Count != 0)
                {
                    infoList.ForEach(item => item.UserId = userid);
                    insertaction(infoList);
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError(string.Format("京东集合入库出现异常:{0},userid:{1}", typeof(T).Name, userid), ex);

                return false;
            }
            return true;
        }
        private void actionTryInsert<T>(T info, int  userid, Action<T> insertaction,bool isempty=false) where T : BaseEntity
        {
            try
            {
                if (info != null&&!isempty)
                {
                    info.UserId = userid;
                    insertaction(info);
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError(typeof(T).Name + "出现异常", ex);
            }
        }
    }
}
