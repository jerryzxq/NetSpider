using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Service
{
    // BasicEntity服务对象
    internal class basicImpl : BaseService<BasicEntity>, IBasic 
    {
        public BasicEntity GetByIdentityNoAndMobile(string IdentityNo, string Mobile)
        {
            var ls = base.Find(@"from BasicEntity where OperationLog.IdentityNo=? and  OperationLog.Mobile=? order by Oper_id desc", new object[] { IdentityNo, Mobile });

            return ls.FirstOrDefault();
        }


        public BasicEntity GetByOperid(int oper_id)
        {
            var ls = base.Find(@"from BasicEntity where Oper_id=?", new object[] { oper_id });
            return ls.FirstOrDefault();
        }
    }
}
	
