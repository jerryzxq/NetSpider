using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
 
namespace Vcredit.ExtTrade.BusinessLayer
{
    /// <summary>
    /// 准贷记卡
    /// </summary>
    public class CRD_CD_STNCARDBusiness  
    {
        BaseDao dao = new BaseDao();
        public List<CRD_CD_STNCARDEntity> GetList(decimal reportid)
        {
            return dao.Select<CRD_CD_STNCARDEntity>(x => x.Report_Id == reportid);
        }
    }
}
