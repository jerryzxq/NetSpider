using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity;
using Vcredit.NetSpider.Emall.Entity.TaoBao;
using ServiceStack.OrmLite;
namespace Vcredit.NetSpider.Emall.Business.JingDong
{
    public class JD_ExpenseRecordBll : Business<ExpenseRecordEntity, SqlConnectionFactory>
    {

        public DateTime? GetMaxTime(string username)
        {
            using (var dbo = DBConnection)
            {
                return dbo.Scalar<DateTime?>(" select max(CostDate) from JD_ExpenseRecord where AccountName=@UserName", new { UserName = username });
            }
        }
    }
}
