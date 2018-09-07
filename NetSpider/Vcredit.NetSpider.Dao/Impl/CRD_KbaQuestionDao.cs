using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Dao;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Dao
{
    // CRD_KbaQuestionEntity数据访问对象
    internal class CRD_KbaQuestionDao : BaseDao<CRD_KbaQuestionEntity>, ICRD_KbaQuestionDao 
    {
        public IDaoHelper<CRD_KbaQuestionEntity> DaoHelper { get; set; }
    }
}
