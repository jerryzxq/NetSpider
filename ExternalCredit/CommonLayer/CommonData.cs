using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Vcredit.ExtTrade.CommonLayer
{
    public class CommonData
    {
        #region 征信计算变量特殊值

       public const int _9999999 = -9999999;
       public const int _8888888 = -8888888;
       public const int _9999995 = -9999995;
       public const int _9999996 = -9999996;
       public const int _9999997 = -9999997;
       public const int _9999998 = -9999998;
       public const int _8888887 = -8888887;
       public const int _8888886 = -8888886;

        #endregion


        #region  Excel对应的所有字段

        public const string CRD_PI_IDENTITY = "身份信息";

        public const string CRD_PI_IDENTITYMate = "配偶信息";

        public const string CRD_PI_RESIDENCE = "居住信息";

        public const string CRD_PI_PROFESSNL = "职业信息";

        public const string CRD_IS_CREDITCUE = "信用提示";

        public const string CRD_IS_CREDITCUEScore = "中征信评分";

        public const string CRD_CD_OverDueBreake = "逾期及违约信息概要";

        public const string CRD_IS_OVDSUMMARY = "逾期(透支)信息汇总";

        public const string CRD_CD_OutstandeSummary = "未结清贷款信息汇总";

        public const string CRD_CD_NoCancellLND = "未销户贷记卡信息汇总";

        public const string CRD_CD_NoCancellSTNCARD = "未销户准贷记卡信息汇总";

        public const string CRD_IS_GRTSUMMARY = "对外担保信息汇总";

        public const string CRD_CD_ASSETDPST = "资产处置信息";

        public const string CRD_CD_ASRREPAY = "保证人代偿信息";

        public const string CRD_CD_LN = "贷款";

        public const string CRD_CD_LND = "贷记卡";

        public const string CRD_CD_STNCARD = "准贷记卡";

        public const string CRD_CD_GUARANTEE = "对外担保信息";

        public const string NewStnCRD_CD_GUARANTEE = "对外信用卡担保信息";

        public const string NewCRD_CD_GUARANTEE = "对外贷款担保信息";

        public const string CRD_PI_TAXARREAR = "欠税记录";

        public const string CRD_PI_CIVILJDGM = "民事判决记录";

        public const string CRD_PI_FORCEEXCTN = "强制执行记录";

        public const string CRD_PI_ADMINPNSHM = "行政处罚记录";

        public const string CRD_PI_ACCFUND = "住房公积金参缴记录";

        public const string CRD_PI_ENDINSDPT = "养老保险金缴存记录";

        public const string CRD_PI_ENDINSDLR = "养老保险金发放记录";

        public const string CRD_PI_SALVATION = "低保救助记录";

        public const string CRD_PI_COMPETENCE = "执业资格记录";

        public const string CRD_PI_ADMINAWARD = "行政奖励记录";

        public const string CRD_PI_VEHICLE = "车辆交易和抵押记录";

        public const string CRD_PI_TELPNT = "电信缴费记录";

        public const string CRD_AN_ANCINFO = "本人声明";

        public const string CRD_AN_DSTINFO = "异议标注";

        public const string CRD_QR_RECORDDTLINFO = "信贷审批查询记录明细";

        public const string CRD_CD_LN_SPL = "贷款特殊交易";

        public const string CRD_CD_LN_OVD = "贷款逾期/透支记录";

        public const string CRD_CD_STN_SPL = "准贷记卡特殊交易";

        public const string CRD_CD_STN_OVD = "准贷记卡逾期/透支记录";

        public const string CRD_CD_LND_SPL = "贷记卡特殊交易";

        public const string CRD_CD_LND_OVD = "贷记卡逾期/透支记录";

        public const string CRD_QR_REORDSMR = "查询记录汇总";

        public const string specialTradeStr = "特殊交易";

        public const string overDueStr = "逾期/透支记录";


        public const string businessTypeStr = "业务类型";

        public const string involveCombineStr = "最近24个月";

        public const string nullstr = "null";

        public const string doubleTrunk = "--";

        public const string zero = "000000";

        public const string creditReportTable = "征信报告主表";

        public const string reportDescription = "报告说明";

        public const string style = "style";

        public const string b = "b";

        public const string tbody = "tbody";

        public const string table = "table";

        public const string tr = "tr";
        public const string reportsn = "报告编号";

        public const string queryTime = "查询请求时间";

        public const string reporttime = "报告时间";

        public const string num = "编号";

        public readonly static DateTime defaultTime = new DateTime(1800, 11, 11);

        public const int defaultNumValue = 0;
        #endregion

        /// <summary>
        /// 统计类型
        /// </summary>
        public readonly static Dictionary<string, string> Type_Ids = new Dictionary<string, string>() {
            {"1","信贷审批最近一个月内的查询机构数"},
            {"2","信用卡审批最近一个月内的查询机构数"},
            {"3","信贷审批最近一个月内的查询次数"},
            {"4","信用卡审批最近一个月内的查询次数"},
            {"5","本人查询最近一个月内的查询次数"},
            {"6","贷后管理最近两年内的查询次数"},
            {"7","担保资格审查最近两年内的查询次数"},
            {"8","特约商户实名审查最近两年内的查询次数"}
        };
        /// <summary>
        /// 业务类型
        /// </summary>
        public readonly static string[] BusinessTypeArray = { CRD_CD_LN, CRD_CD_LND, CRD_CD_STNCARD };
        /// <summary>
        /// 逾期类型
        /// </summary>
        public readonly static string[] OveDueType = { "贷款逾期", "贷记卡逾期", "准贷记卡60天以上透支" };

   

        #region 证件类型

        /// <summary>
        /// 证件类型
        /// </summary>
        public readonly static Dictionary<string, string> certTypeDic = new Dictionary<string, string>(){
            {"0","身份证"},
            {"1","户口本"},
            {"2","护照"},
            {"3","军官证"},
            {"4","士兵证"},
            {"5","港澳居民来往内地通行证"},
            {"6","台湾同胞来往内地通行证"},
            {"7","临时身份证"},
            {"8","外国人居留证"},
            {"9","警官证"},
            {"A","香港身份证"},
            {"B","澳门身份证"},
            {"C","台湾身份证"},
            {"X","其他证件"}
        };

        public const string 身份证 = "0";
        public const string 户口本 = "1";
        public const string 护照 = "2";
        public const string 军官证 = "3";
        public const string 士兵证 = "4";
        public const string 港澳居民来往内地通行证 = "5";
        public const string 台湾同胞来往内地通行证 = "6";
        public const string 临时身份证 = "7";
        public const string 外国人居留证 = "8";
        public const string 警官证 = "9";
        public const string 香港身份证 = "A";
        public const string 澳门身份证 = "B";
        public const string 台湾身份证 = "C";
        public const string 其他证件 = "X";


        #endregion

    

        

        /// <summary>
        /// 担保征信查询原因
        /// </summary>
        public readonly static Dictionary<string, string> AssureQueryReason = new Dictionary<string, string>(){
           {"01","贷后管理"},
           {"02","贷款审批"},
           {"03","担保资格查询"}
        };

        public readonly static string LoanAfterManager = "01";//贷后管理
        public readonly static string LoanApproval = "02";//贷款审批
        public readonly static string GuaranteeQualification = "03";//担保资格查询

        #region  产品名称
        public const string XINLOUDAI = "XINLOUDAI";
        #endregion


        public readonly static string LineAuthorization = "01";//线上授权
        public readonly static string OffLineAuthorization = "02";//线下授权
    }
}
