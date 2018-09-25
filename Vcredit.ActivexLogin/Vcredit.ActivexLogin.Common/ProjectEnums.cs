using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ActivexLogin.Common
{
    public static class ProjectEnums
    {
        /// <summary>
        /// 站点类型
        /// </summary>
        public enum WebSiteType
        {
            #region 公积金

            /// <summary>
            /// 广州公积金
            /// </summary>
            [Description("广州公积金")]
            GuangZhouGjj = 1001,

            /// <summary>
            /// 天津公积金
            /// </summary>
            [Description("天津公积金")]
            TianJinGjj = 1002,

            /// <summary>
            /// 深圳公积金
            /// </summary>
            [Description("深圳公积金")]
            ShenZhenGjj = 1003,

            /// <summary>
            /// 武汉公积金
            /// </summary>
            [Description("武汉公积金")]
            WuHanGjj = 1004,

            #endregion

            #region 银行
            /// <summary>
            /// 招商银行
            /// </summary>
            [Description("招商银行")]
            CmbBank = 2001,

            /// <summary>
            /// 招商银行
            /// </summary>
            [Description("农业银行")]
            AbcBank = 2002,

            /// <summary>
            /// 广发银行
            /// </summary>
            [Description("广发银行")]
            CgbBank = 2003,

            /// <summary>
            /// 中国银行
            /// </summary>
            [Description("中国银行")]
            BocBank = 2004,

            /// <summary>
            /// 光大银行
            /// </summary>
            [Description("光大银行")]
            CebBank = 2005,

            /// <summary>
            /// 成都银行
            /// </summary>
            [Description("成都银行")]
            BocdBank = 2006,

            /// <summary>
            /// 中信银行
            /// </summary>
            [Description("中信银行")]
            CiticBank = 2007,

            /// <summary>
            /// 民生银行
            /// </summary>
            [Description("民生银行")]
            CmbcBank = 2008,

            /// <summary>
            /// 交通银行
            /// </summary>
            [Description("交通银行")]
            CommBank = 2009,

            /// <summary>
            /// 福建农信
            /// </summary>
            [Description("福建农信")]
            FjnxBank = 2010,

            /// <summary>
            /// 广州农商
            /// </summary>
            [Description("广州农商")]
            GznsBank = 2011,

            /// <summary>
            /// 杭州银行
            /// </summary>
            [Description("杭州银行")]
            HzBank = 2012,

            /// <summary>
            /// 华夏银行
            /// </summary>
            [Description("华夏银行")]
            HxBank = 2013,

            /// <summary>
            /// 工商银行
            /// </summary>
            [Description("工商银行")]
            IcbcBank = 2014,

            /// <summary>
            /// 江苏银行
            /// </summary>
            [Description("江苏银行")]
            JsBank = 2015,

            /// <summary>
            /// 宁波银行
            /// </summary>
            [Description("宁波银行")]
            NbcBank = 2016,

            /// <summary>
            /// 邮储银行
            /// </summary>
            [Description("邮储银行")]
            PsbcBank = 2017,

            /// <summary>
            /// 上海银行
            /// </summary>
            [Description("上海银行")]
            ShhBank = 2018,

            /// <summary>
            /// 平安银行
            /// </summary>
            [Description("平安银行")]
            PinganBank = 2019,

            #endregion

            #region 征信
            /// <summary>
            /// 人行网络版征信
            /// </summary>
            [Description("人行网络版征信")]
            RenHangNetWorkCredit = 3001,

            #endregion
        }

    }
}
