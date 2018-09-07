using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
 
namespace Vcredit.ExtTrade.BusinessLayer
{
    /// <summary>
    /// 贷记卡
    /// </summary>
    public class CRD_CD_LNDBusiness  
    {
        BaseDao dao = new BaseDao();
        public List<CRD_CD_LNDEntity> GetList(decimal reportid)
        {
            var list= dao.Select<CRD_CD_LNDEntity>(x => x.Report_Id == reportid);
            foreach (var item in list)
            {
                item.LndoverList = dao.Select<CRD_CD_LND_OVDEntity>(x => x.Card_Id == item.Loancard_Id);
            }
            return list;
        }
    }
}
