using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Vcredit.NetSpider.RestService.Models.RC
{
    /// <summary>
    /// 决策结果
    /// </summary>
    [Serializable]
    public class DecisionResult
    {
        /// <summary>
        /// 规则ID
        /// </summary>
        public int ModId { get; set; }

        /// <summary>
        /// 规则名称
        /// </summary>
        public string ModName { get; set; }

        /// <summary>
        /// 版本Id
        /// </summary>
        public int VerId { get; set; }

        /// <summary>
        /// 结果
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 导致拒绝的规则组
        /// </summary>
        public SerializableDictionary<string, string> RejectReasonList { get; set; }

        /// <summary>
        /// 规则运行结果集
        /// </summary>
        public SerializableDictionary<int, DecisionResultDetail> RuleResultSets { get; set; }

        /// <summary>
        /// 可以显示的规则运行结果集
        /// </summary>
        public SerializableDictionary<string, string> RuleResultCanShowSets { get; set; }
    }

    [Serializable]
    public class DecisionVBResult
    {
        /// <summary>
        /// vb结果集合
        /// </summary>
        public DataTable ResultData { get; set; }

        /// <summary>
        /// VB_ID 与 VB_NM对应字典
        /// </summary>
        public SerializableDictionary<int, VBInfo> VBSets { get; set; }

    }

    public class VBInfo
    {
        /// <summary>
        /// VB_NM
        /// </summary>		
        private string _vb_nm;
        public string VB_NM
        {
            get { return _vb_nm; }
            set { _vb_nm = value; }
        }

        /// <summary>
        /// VB_TP
        /// </summary>		
        private int _vb_tp;
        public int VB_TP
        {
            get { return _vb_tp; }
            set { _vb_tp = value; }
        }

    }

    public class DecisionResultConst
    {
        /// <summary>
        /// 拒绝
        /// </summary>
        public static string FailResult = "建议拒绝";

        /// <summary>
        /// 成功
        /// </summary>
        public static string SucessResult = "通过";

    }

    [Serializable]
    public class DecisionResultDetail
    {
        /// <summary>
        /// 规则
        /// </summary>
        public VW_MOD_RU Rule { get; set; }

        /// <summary>
        /// 结果
        /// </summary>
        public string Result { get; set; }
    }
}
