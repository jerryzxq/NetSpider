using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
 
namespace Vcredit.ExtTrade.BusinessLayer
{
    /// <summary>
    /// 报告主表业务处理
    /// </summary>
    public class CRD_HD_REPORTBusiness  
    {
        BaseDao dao = new BaseDao();

        public  CRD_HD_REPORTEntity Get(decimal  reportid)
        {
            return dao.SingleById<CRD_HD_REPORTEntity>(reportid);
            
        }
        public CRD_HD_REPORTEntity Get(string report_Sn)
        {
            return dao.Select<CRD_HD_REPORTEntity>(x => x.Report_Sn == report_Sn).FirstOrDefault();
        }
        public CRD_HD_REPORTEntity GetBycertno(string Cert_No)
        {
            var list= dao.Select<CRD_HD_REPORTEntity>(x => x.Cert_No == Cert_No);
            if(list.Count!=0)
            {
                return list.OrderByDescending(x => x.Report_Id).First();
            }
            return null;
        }
    }
}
