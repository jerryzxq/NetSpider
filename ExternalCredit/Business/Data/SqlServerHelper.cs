using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ServiceStack.OrmLite;

namespace Vcredit.ExtTrade.BusinessLayer
{
    public class SqlServerHelper : IDBHelper
    {

        string ConnectionName;
        public SqlServerHelper(string ConnectionName)
        {
            this.ConnectionName = ConnectionName;
           
        }
        public SqlServerHelper()
        {

        }
        public string ConnectionString
        {
            get
            {
                return ConnectionStringConfig();
            }
            set
            {
                this.ConnectionString = value;
            }
        }

        public IOrmLiteDialectProvider Provider
        {
            get
            {
                return SqlServerDialect.Provider;
            }

        }

        private String ConnectionStringConfig()
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString;
        }

        public OrmLiteConnectionFactory ConnectFactory
        {
            get
            {
                return new OrmLiteConnectionFactory(this.ConnectionString, this.Provider);
            }
        }
    }
}
