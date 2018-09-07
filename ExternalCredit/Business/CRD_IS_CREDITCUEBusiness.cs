using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.ModelLayer;
 
namespace Vcredit.ExtTrade.BusinessLayer
{
    /// <summary>
    /// 信息提示业务处理
    /// </summary>
    public class CRD_IS_CREDITCUEBusiness  
    {
        BaseDao dao = new BaseDao();

        public CRD_IS_CREDITCUEEntity GetEntity(decimal reportID)
        {
           return  dao.Select<CRD_IS_CREDITCUEEntity>(x => x.Report_Id == reportID).FirstOrDefault();
        }
    }
}
