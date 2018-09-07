using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    public class SqlServerSchema : IDBSchema
    {
        public string ConnectionString = "Server=10.138.60.88;database=emall;uid=zhangzhibo;pwd=zhangzhibo123;";

        public SqlConnection conn;

        public SqlServerSchema()
        {
            conn = new SqlConnection(ConnectionString);
            conn.Open();
        }

        public List<TableDBSchema> GetTablesList()
        {
            DataTable dt = conn.GetSchema("Tables");
            List<TableDBSchema> list = new List<TableDBSchema>();
            foreach (DataRow row in dt.Rows)
            {
                var table = new TableDBSchema()
                {
                    Schema = row["TABLE_SCHEMA"].ToString(),
                    TableName = row["TABLE_NAME"].ToString()
                };
                list.Add(table);
            }
            return list;
        }

        public Table GetTableMetadata(string tableName)
        {
            string selectCmdText = string.Format("SELECT * FROM {0} where 1<>1", tableName); ;
            SqlCommand command = new SqlCommand(selectCmdText, conn);
            SqlDataAdapter ad = new SqlDataAdapter(command);
            System.Data.DataSet ds = new DataSet();
            ad.FillSchema(ds, SchemaType.Mapped, tableName);

            Table table = new Table(ds.Tables[0]);
            Dispose();
            return table;
        }

        public List<ColumnDescription> GetCoumnDescriptionsData(TableDBSchema dbtable)
        {
            List<ColumnDescription> list = new List<ColumnDescription>();
            string selectCmdText = @"SELECT  
                [TableSchema]=OBJECT_SCHEMA_NAME(c.object_id),
                [TableName] = OBJECT_NAME(c.object_id), 
                [ColumnName] = c.name, 
                [Description] = ex.value  
                FROM  
                sys.columns c  
                LEFT OUTER JOIN  
                sys.extended_properties ex  
                ON  
                ex.major_id = c.object_id 
                AND ex.minor_id = c.column_id  
                AND ex.name = 'MS_Description'  
                WHERE  
                OBJECTPROPERTY(c.object_id, 'IsMsShipped')=0  ";

            selectCmdText += " AND OBJECT_NAME(c.object_id) = '" + dbtable.TableName + "'";
            selectCmdText += "  AND OBJECT_SCHEMA_NAME(c.object_id) = '" + dbtable.Schema + "'";
            selectCmdText += @" ORDER  
                BY OBJECT_NAME(c.object_id), c.column_id";
            SqlCommand command = new SqlCommand(selectCmdText, conn);

            conn.Open();

            SqlDataReader read = command.ExecuteReader(CommandBehavior.CloseConnection);
            while (read.Read())
            {
                ColumnDescription column = new ColumnDescription()
                {
                    TableSchema = read.GetString(0),
                    TableName = read.GetString(1),
                    ColumnName = read.GetString(2),
                    Description = read.IsDBNull(3) ? "" : read.GetString(3)

                };
                list.Add(column);
            }
            Dispose();
            return list;
        }
        public void Dispose()
        {
            if (conn != null)
                conn.Close();
        }
    }
    public class MySqlSchema : IDBSchema
    {
        public string ConnectionString = "Server=10.138.60.88;database=emall;uid=zhangzhibo;pwd=zhangzhibo123;";

        public MySqlConnection conn;

        public MySqlSchema()
        {
            conn = new MySqlConnection(ConnectionString);
            conn.Open();
        }

        public List<TableDBSchema> GetTablesList()
        {
            DataTable dt = conn.GetSchema("Tables");
            List<TableDBSchema> list = new List<TableDBSchema>();
            foreach (DataRow row in dt.Rows)
            {

                var table = new TableDBSchema()
                {
                    Schema = row["TABLE_SCHEMA"].ToString(),
                    TableName = row["TABLE_NAME"].ToString()
                };
                list.Add(table);
            }
            return list;
        }

        public Table GetTableMetadata(string tableName)
        {
            string selectCmdText = string.Format("SELECT * FROM {0}", tableName); ;
            MySqlCommand command = new MySqlCommand(selectCmdText, conn);
            MySqlDataAdapter ad = new MySqlDataAdapter(command);
            System.Data.DataSet ds = new DataSet();
            ad.FillSchema(ds, SchemaType.Mapped, tableName);

            Table table = new Table(ds.Tables[0]);
            return table;
        }

        public List<ColumnDescription> GetCoumnDescriptionsData(TableDBSchema dbtable)
        {
            return null;
        }
        public void Dispose()
        {
            if (conn != null)
                conn.Close();
        }
    }
    public class Table
    {
        public Table(DataTable t)
        {
            this.PKs = this.GetPKList(t);
            this.Columns = this.GetColumnList(t);
            this.ColumnTypeNames = this.SetColumnNames();
        }

        public List<Column> PKs;

        public List<Column> Columns;

        public string ColumnTypeNames;
        public List<Column> GetPKList(DataTable dt)
        {
            List<Column> list = new List<Column>();
            Column c = null;
            if (dt.PrimaryKey.Length > 0)
            {
                list = new List<Column>();
                foreach (DataColumn dc in dt.PrimaryKey)
                {
                    c = new Column(dc);
                    list.Add(c);
                }
            }
            return list;
        }

        private List<Column> GetColumnList(DataTable dt)
        {
            List<Column> list = new List<Column>();
            Column c = null;
            foreach (DataColumn dc in dt.Columns)
            {
                c = new Column(dc);
                list.Add(c);
            }
            return list;
        }

        private string SetColumnNames()
        {
            List<string> list = new List<string>();
            foreach (Column c in this.Columns)
            {
                list.Add(string.Format("{0} {1}", c.TypeName, c.LowerColumnName));
            }
            return string.Join(",", list.ToArray());
        }
    }

    public class ColumnDescription
    {
        public string TableSchema { set; get; }
        public string TableName { set; get; }
        public string ColumnName { set; get; }
        public string Description { set; get; }


    }

    public class Column
    {
        DataColumn columnBase;

        public Column(DataColumn columnBase)
        {
            this.columnBase = columnBase;
        }

        public string ColumnName { get { return this.columnBase.ColumnName; } }

        public string MaxLength { get { return this.columnBase.MaxLength.ToString(); } }

        public bool IsNull { get { return this.columnBase.AllowDBNull; } }
        public bool AutoIncrement { get { return this.columnBase.AutoIncrement; } }
        public string TypeName
        {
            get
            {
                string result = string.Empty;
                switch (this.columnBase.DataType.Name)
                {
                    case "Guid":
                    case "String":
                        result = "string";
                        break;
                    case "Int32":
                        result = "int";
                        break;
                    case "Int64":
                        result = "long";
                        break;
                    case "Decimal":
                    case "Money":
                        result = "decimal";
                        break;
                    case "Boolean":
                        result = "bool";
                        break;
                    case "Date":
                    case "DateTime":
                    case "DateTime2":
                        result = "DateTime";
                        break;
                    case "Time":
                        result = "TimeSpan";
                        break;
                    case "Byte":
                        result = "byte";
                        break;
                    case "DateTimeOffset":
                        result = "DateTimeOffset";
                        break;
                    case "Byte[]":
                        result = "byte[]";
                        break;
                    case "Single":
                        result = "Single";
                        break;
                    case "Double":
                        result = "double";
                        break;
                    default:
                        result = this.columnBase.DataType.Name;
                        break;

                }


                return result;
            }
        }

        public bool AllowDBNull { get { return this.columnBase.AllowDBNull; } }

        public string UpColumnName
        {
            get
            {
                return string.Format("{0}{1}", this.ColumnName[0].ToString().ToUpper(), this.ColumnName.Substring(1));
            }
        }

        public string LowerColumnName
        {
            get
            {
                return string.Format("{0}{1}", this.ColumnName[0].ToString().ToLower(), this.ColumnName.Substring(1));
            }
        }
    }

    public class GeneratorHelper
    {
        public static string GetQuesMarkByType(Column column)
        {
            string result = column.TypeName;
            if (column.IsNull && column.TypeName != "string")
            {
                result += "?";
            }
            return result;
        }

        public static string GetColumnDection(Column column, List<ColumnDescription> decs)
        {
            var dection = decs.Where(e => e.ColumnName == column.ColumnName).FirstOrDefault();

            return dection != null ? dection.Description : "";
        }
    }


    public class DBSchemaFactory
    {
        static readonly string DatabaseType = "MySql";
        public static IDBSchema GetDBSchema()
        {
            IDBSchema dbSchema;
            switch (DatabaseType)
            {
                case "SqlServer":
                    {
                        dbSchema = new SqlServerSchema();
                        break;
                    }
                case "MySql":
                    {
                        dbSchema = new MySqlSchema();
                        break;
                    }
                default:
                    {
                        throw new ArgumentException("The input argument of DatabaseType is invalid!");
                    }
            }
            return dbSchema;
        }
    }

    public interface IDBSchema : IDisposable
    {
        List<TableDBSchema> GetTablesList();

        Table GetTableMetadata(string tableName);

        List<ColumnDescription> GetCoumnDescriptionsData(TableDBSchema dbtable);
    }

    public class TableDBSchema
    {
        string schema;
        string tableName;
        public string Schema
        {
            get { return schema; }
            set { schema = value; }

        }
        public string TableName
        {
            get
            {
                return tableName;
            }
            set
            {
                tableName = value;

            }
        }

    }
}
