using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.ModelLayer;
 
namespace Vcredit.ExtTrade.BusinessLayer
{
    /// <summary>
    /// 民事判决记录
    /// </summary>
    public class CRD_PI_CIVILJDGMBusiness  
    {
        BaseDao dao = new BaseDao();
        public List<CRD_PI_CIVILJDGMEntity> GetList(int reportid)
        {
            var list = dao.Select<CRD_PI_CIVILJDGMEntity>(x => x.Report_Id == reportid);
            return list;
        }
    }
}
