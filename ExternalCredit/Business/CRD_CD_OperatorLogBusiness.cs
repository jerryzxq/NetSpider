using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer;

namespace Vcredit.ExtTrade.BusinessLayer
{
    public   class CRD_CD_OperatorLogBusiness
    {
        BaseDao dao = new BaseDao();
        public void InsertEntity(CRD_CD_OperatorLogEntity ope)
        {
            try
            {
                dao.Insert(ope);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("查询日志插入出错", ex);
            }
        }
    }
}
