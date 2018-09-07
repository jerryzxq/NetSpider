using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.ModelLayer;
 
namespace Vcredit.ExtTrade.BusinessLayer
{
    /// <summary>
    /// 强制执行记录
    /// </summary>
    public class CRD_PI_FORCEEXCTNBusiness  
    {
        BaseDao dao = new BaseDao();
        public List<CRD_PI_FORCEEXCTNEntity> GetList(int reportid)
        {
            var list = dao.Select<CRD_PI_FORCEEXCTNEntity>(x => x.Report_Id == reportid);
            return list;
        }
    }
}
