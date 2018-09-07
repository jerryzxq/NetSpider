using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
 
namespace Vcredit.ExtTrade.BusinessLayer
{
    /// <summary>
    /// 信贷审批查询记录明细
    /// </summary>
    public class CRD_QR_RECORDDTLINFOBusiness  
    {
        BaseDao dao = new BaseDao();
        public List<CRD_QR_RECORDDTLINFOEntity> GetList(decimal reportid)
        {
            return dao.Select<CRD_QR_RECORDDTLINFOEntity>(x => x.Report_Id == reportid);
        }
       
    }
}
