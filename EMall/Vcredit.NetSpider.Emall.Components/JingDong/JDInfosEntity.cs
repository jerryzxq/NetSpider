using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  Vcredit.NetSpider.Emall.Entity
{
    public class JDInfosEntity   //基本信息
    {
        /// <summary>
        /// 回调的url
        /// </summary>
        public string  CallBackURL{ get; set; }
        /// <summary>
        /// 抓取信息类型
        /// </summary>
        public string CrawlerTypes{ get; set; }
        #region 商城账户信息

        private UserInfoEntity userInfo = new UserInfoEntity();
        /// <summary>
        /// 用户信息
        /// </summary>

        public UserInfoEntity UserInfo
        {
            get { return userInfo; }
            set { userInfo = value; }
        }

        private List<GrowthValueDetailEntity> _GrowthValueDetail = new List<GrowthValueDetailEntity>();
        /// <summary>
        /// 成长值详情
        /// </summary>
        public List<GrowthValueDetailEntity> GrowthValueDetail
        {
            get { return _GrowthValueDetail; }
            set { _GrowthValueDetail = value; }
        }


        private List<ReceiveAddressEntity> _ReceiveAddress = new List<ReceiveAddressEntity>();
        /// <summary>
        /// 收货地址集合
        /// </summary>
        public List<ReceiveAddressEntity> ReceiveAddress
        {
            get { return _ReceiveAddress; }
            set { _ReceiveAddress = value; }
        }





        private List<ExpenseRecordEntity> _ExpenseRecord = new List<ExpenseRecordEntity>();
        /// <summary>
        /// 消费记录集合
        /// </summary>
        public List<ExpenseRecordEntity> ExpenseRecord
        {
            get { return _ExpenseRecord; }
            set { _ExpenseRecord = value; }
        }

        private List<ShareDetailEntity> _ShareDetail = new List<ShareDetailEntity>();
        /// <summary>
        /// 分享设计网站集合
        /// </summary>
        public List<ShareDetailEntity> ShareList
        {
            get { return _ShareDetail; }
            set { _ShareDetail = value; }
        }


        private List<QuickPaymentEntity> _QuickPayment = new List<QuickPaymentEntity>();
        /// <summary>
        /// 快捷支付集合
        /// </summary>
        public List<QuickPaymentEntity> QuickPayment
        {
            get { return _QuickPayment; }
            set { _QuickPayment = value; }
        }

        #endregion

        #region 商城订单信息
        private List<OrderEntity> _OrderList = new List<OrderEntity>();
        /// <summary>
        /// 订单集合
        /// </summary>
        public List<OrderEntity> OrderList
        {
            get { return _OrderList; }
            set { _OrderList = value; }
        }

     


        private List<BrowseHistoryEntity> _BrowseHistory = new List<BrowseHistoryEntity>();
        /// <summary>
        /// 浏览集合
        /// </summary>
        public List<BrowseHistoryEntity> BrowseHistory
        {
            get { return _BrowseHistory; }
            set { _BrowseHistory = value; }
        }



        private List<ShoppingCartEntity> _ShppingCartGoodsList = new List<ShoppingCartEntity>();
        /// <summary>
        ///购物车商品
        /// </summary>
        public List<ShoppingCartEntity> ShppingCartGoodsList
        {
            get { return _ShppingCartGoodsList; }
            set { _ShppingCartGoodsList = value; }
        }

        #endregion

        #region 京东白条信息
        /// <summary>
        /// 京东白条账户信息
        /// </summary>
        private JDWhiteBarEntity _JDWhiteBar = new JDWhiteBarEntity();

        public JDWhiteBarEntity JDWhiteBar
        {
            get { return _JDWhiteBar; }
            set { _JDWhiteBar = value; }
        }


        private List<WhiteAllOrderEntity> _WhiteAllOrderList = new List<WhiteAllOrderEntity>();
        /// <summary>
        /// 全部
        /// </summary>
        public List<WhiteAllOrderEntity> WhiteAllOrderList
        {
            get { return _WhiteAllOrderList; }
            set { _WhiteAllOrderList = value; }
        }
        private List<InstalmentEntity> _InstalmentList = new List<InstalmentEntity>();
        /// <summary>
        /// 订单分期付款详细信息
        /// </summary>
        public List<InstalmentEntity> InstalmentList
        {
            get { return _InstalmentList; }
            set { _InstalmentList = value; }
        }


        private List<WhiteNeedPayEntity> _WhiteNeedPayList = new List<WhiteNeedPayEntity>();
        /// <summary>
        /// 待还款
        /// </summary>
        public List<WhiteNeedPayEntity> WhiteNeedPayList
        {
            get { return _WhiteNeedPayList; }
            set { _WhiteNeedPayList = value; }
        }


        private List<WhiteRefundedEntity> _WhiteRefundedList = new List<WhiteRefundedEntity>();
        /// <summary>
        /// 已退款
        /// </summary>
        public List<WhiteRefundedEntity> WhiteRefundedList
        {
            get { return _WhiteRefundedList; }
            set { _WhiteRefundedList = value; }
        }


        private List<WhiteRepayMentEntity> _WhiteRepayMentList = new List<WhiteRepayMentEntity>();
        /// <summary>
        /// 已还款
        /// </summary>
        public List<WhiteRepayMentEntity> WhiteRepayMentList
        {
            get { return _WhiteRepayMentList; }
            set { _WhiteRepayMentList = value; }
        }

        #endregion

        #region 关注数据
        /// <summary>
        /// 关注商品
        /// </summary>
        private List<FocusProductEntity> _FocusProductList = new List<FocusProductEntity>();

        public List<FocusProductEntity> FocusProductList
        {
            get { return _FocusProductList; }
            set { _FocusProductList = value; }
        }
        /// <summary>
        /// 关注店铺
        /// </summary>
        private List<FocusShopEntity> _FocusShopList = new List<FocusShopEntity>();

        public List<FocusShopEntity> FocusShopList
        {
            get { return _FocusShopList; }
            set { _FocusShopList = value; }
        }
        /// <summary>
        /// 关注品牌
        /// </summary>
        private List<FocusBrandEntity> _FocusBrandList = new List<FocusBrandEntity>();

        public List<FocusBrandEntity> FocusBrandList
        {
            get { return _FocusBrandList; }
            set { _FocusBrandList = value; }
        }
        /// <summary>
        /// 关注活动
        /// </summary>
        private List<FocusActivityEntity> _FocusActivityList = new List<FocusActivityEntity>();

        public List<FocusActivityEntity> FocusActivityList
        {
            get { return _FocusActivityList; }
            set { _FocusActivityList = value; }
        }
        #endregion

    }
}
