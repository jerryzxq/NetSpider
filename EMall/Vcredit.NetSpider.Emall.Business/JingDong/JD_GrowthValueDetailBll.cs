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
    public class JD_GrowthValueDetailBll : Business<GrowthValueDetailEntity, SqlConnectionFactory>
    {
        public DateTime? GetMaxTime(string username)
        {
            using (var dbo = DBConnection)
            {
                return dbo.Scalar<DateTime?>(" select max(HappenDate) from JD_GrowthValueDetail where AccountName=@UserName", new { UserName = username });
            }
        }
    }
}
