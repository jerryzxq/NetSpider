using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.CrawlerLayer.CreditVariable
{
    public class VariableInfo
    {
        public string Report_Sn { get; set; }
        public decimal Report_Id { get; set; }
        public DateTime? Query_Time { get; set; }
        /// <summary>
        /// 报告时间
        /// </summary>
        public DateTime? Report_Create_Time { get; set; }
        /// <summary>
        /// 被查询者姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 被查询者证件类型
        /// </summary>
        public string Cert_Type { get; set; }
        /// <summary>
        /// 被查询者证件号码
        /// </summary>
        public string Cert_No { get; set; }
        CRD_STAT_LNEntity stat_ln = new CRD_STAT_LNEntity();

        public CRD_STAT_LNEntity CRD_STAT_LN
        {
            get { return stat_ln; }
            set { stat_ln = value; }
        }
        CRD_STAT_LNDEntity stat_lnd = new CRD_STAT_LNDEntity();

        public CRD_STAT_LNDEntity CRD_STAT_LND
        {
            get { return stat_lnd; }
            set { stat_lnd = value; }
        }
        CRD_STAT_QREntity stat_record = new CRD_STAT_QREntity();

        public CRD_STAT_QREntity CRD_STAT_QR
        {
            get { return stat_record; }
            set { stat_record = value; }
        }

        private PublicInformationSummary _PubInfoSummary = new PublicInformationSummary();
        public virtual PublicInformationSummary PubInfoSummary
        {
            get { return _PubInfoSummary; }
            set { this._PubInfoSummary = value; }
        }

        public bool IsCreditBlank { get; set; }
    }

}
