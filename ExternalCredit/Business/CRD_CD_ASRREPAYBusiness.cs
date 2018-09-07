using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
 
namespace Vcredit.ExtTrade.BusinessLayer
{
    /// <summary>
    /// 保证人代偿信息
    /// </summary>
    public class CRD_CD_ASRREPAYBusiness 
    {
        BaseDao dao = new BaseDao();

        public List<CRD_CD_ASRREPAYEntity> GetList(decimal  reportid)
        {
           return  dao.Select<CRD_CD_ASRREPAYEntity>(x => x.Report_Id == reportid);
        }
    }
}
