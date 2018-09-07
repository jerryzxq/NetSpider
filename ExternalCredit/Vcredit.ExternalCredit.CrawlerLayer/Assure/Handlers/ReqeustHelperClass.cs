using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.CrawlerLayer.Assure
{

    /// <summary>
    /// 人行登陆返回结果
    /// </summary>
    public class AssureLoginResult
    {
        public string errors { get; set; }
        public bool success { get; set; }
    }

    #region 人行担保征信查询结果实体（普通请求）
    /// <summary>
    /// 人行担保征信查询结果实体（普通请求）
    /// </summary>
    public class ReportResult
    {
        public int total { get; set; }
        public DataEntitty[] root { get; set; }
        public bool success { get; set; }
    }

    public class DataEntitty
    {
        public string itype { get; set; }
        public string sname { get; set; }
        public string iid { get; set; }
        public string sorgcode { get; set; }
        public string stoporgcode { get; set; }
        public int istate { get; set; }
        public string susername { get; set; }
        public string iuserid { get; set; }
        public string dapplytimemin { get; set; }
        public string dapplytimemax { get; set; }
        public string sentorgcode { get; set; }
        public string spersonorgcode { get; set; }
        public string dapplytime { get; set; }
        public string sloancard { get; set; }
        public string sentreason { get; set; }
        public string scardtype { get; set; }
        public string scardno { get; set; }
        public string spersonreason { get; set; }
        public string ichildstate { get; set; }
        public string iimportid { get; set; }
        public string ssubmitcode { get; set; }
        public string ssubmittype { get; set; }
        public string iblock { get; set; }
        public string dfeedbacktime { get; set; }
        public string sformat { get; set; }
        public string squerykind { get; set; }
        public string iquerytype { get; set; }
        public string sloancardpwd { get; set; }
        public string sreason { get; set; }
        public string screditreportno { get; set; }
        public string screditreportfilepath { get; set; }
        public string sauthorizationfilepath { get; set; }
        public string isyncid { get; set; }
        public string stoporgname { get; set; }
        public string serror { get; set; }
        public string suserpwd { get; set; }
        public string swarning { get; set; }
        public string sdealorgname { get; set; }
        public string action { get; set; }
        public string columnnameoproccurdate { get; set; }
        public string start { get; set; }
        public string limit { get; set; }
    }
    #endregion

    /// <summary>
    /// 担保返回征信状态值
    /// </summary>
    public enum ReportStatus
    {
        等待查询结果 = 1,
        查询成功 = 3,
        查询失败 = 4,
    }

}
