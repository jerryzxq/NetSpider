using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExternalCredit.CommonLayer.Extension;
namespace Vcredit.ExtTrade.ModelLayer.Nolmal.NewForeignTradeModel
{
    public class SingleCreditReq 
    {
        /// <summary>
        /// 身份证过期日期
        /// </summary>
        public string  Cert_ExpDate { get; set; }

             
        /// <summary>
        /// 合作机构号
        /// </summary>
        public string brNo { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string cifName { get; set; }
        /// <summary>
        /// 证件类型
        /// </summary>
        public string idType { get; set; }
        /// <summary>
        /// 证件号码
        /// </summary>
        public string idNo { get; set; }
        /// <summary>
        /// 查征流水号
        /// </summary>
        public string czPactNo { get; set; }
        /// <summary>
        /// 进件日期
        /// </summary>
        public string appDate { get; set; }
        /// <summary>
        /// 是否有查征授权
        /// </summary>
        public string czAuth { get; set; }
        /// <summary>
        /// 是否身份证信息
        /// </summary>
        public string czId { get; set; }

        /// <summary>
        ///业务类型
        /// </summary>
        public string BusType { get; set; }
        /// <summary>
        /// 查询原因
        /// </summary>
        public string crpReason { get; set; }
        /// <summary>
        /// 申请ApplyID
        /// </summary>
        public int ApplyID { get; set; }

        public string grantType { get; set; }

    }
}
