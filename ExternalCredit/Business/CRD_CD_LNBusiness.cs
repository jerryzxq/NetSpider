using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
 
namespace Vcredit.ExtTrade.BusinessLayer
{
    /// <summary>
    /// 贷款
    /// </summary>
    public class CRD_CD_LNBusiness  
    {
        BaseDao dao = new BaseDao();
        public List<CRD_CD_LNEntity> GetList(decimal reportid)
        {
            var list = dao.Select<CRD_CD_LNEntity>(x => x.Report_Id == reportid);
            foreach (var item in list)
            {
                item.LnoverList = dao.Select<CRD_CD_LN_OVDEntity>(x => x.Card_Id == item.Loan_Id);
            }
            return list;
        }
    }
}
