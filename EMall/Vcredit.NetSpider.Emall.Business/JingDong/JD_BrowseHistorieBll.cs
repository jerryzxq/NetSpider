using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity;
using ServiceStack.OrmLite;

namespace Vcredit.NetSpider.Emall.Business.JingDong
{
    public class JD_BrowseHistorieBll : Business<BrowseHistoryEntity, SqlConnectionFactory>
    {
        public  DateTime? GetMaxTime(string username)
        {
            using(var dbo=DBConnection)
            {
                return dbo.Scalar<DateTime?>(" select max(BrowseDateTime) from JD_BrowseHistorie where AccountName=@UserName", new { UserName = username });
            }
        }
    }
}
