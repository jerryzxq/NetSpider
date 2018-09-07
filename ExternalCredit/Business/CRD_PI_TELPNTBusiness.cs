using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.ModelLayer;
 
namespace Vcredit.ExtTrade.BusinessLayer
{
    /// <summary>
    /// 电信缴费记录
    /// </summary>
    public class CRD_PI_TELPNTBusiness  
    {
        BaseDao dao = new BaseDao();
        public List<CRD_PI_TELPNTEntity> GetList(int reportid)
        {
            var list = dao.Select<CRD_PI_TELPNTEntity>(x => x.Report_Id == reportid);
            return list;
        }
    }
}
