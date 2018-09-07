using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.OrmLite;

namespace Vcredit.ExtTrade.BusinessLayer
{
    public interface IDBHelper
    {
        string ConnectionString { get; set; }
        IOrmLiteDialectProvider Provider { get; }
        OrmLiteConnectionFactory ConnectFactory { get; }
    }
}
