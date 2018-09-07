using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace Vcredit.ExtTrade.CommonLayer
{
   public  class TableMapData
   {
        static  List<TableMapEntity> maplist = new List<TableMapEntity>();
        readonly static object obj = new object(); 
        /// <summary>
        /// 征信报告与数据库字段映射数据
        /// </summary>
        public static List<TableMapEntity> MapList
        {
            get
            {
                if (maplist == null || maplist.Count == 0)
                {
                    lock (obj)
                    {
                        if (maplist == null || maplist.Count == 0)
                        {
                      
                            DataTable dt = NPOIHelper.GetDataTable(ConfigData.MappingTablePath,true);
                            try
                            {
                                maplist = DataTableEntityBuilder<TableMapEntity>.ConvertToList(dt);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                }
                return maplist;
            }

        }
        public static string GetFileName(string tableName, string columnName)
        {
 
            var dbFileNameEntity = MapList.
                Where(item => item.ReportTableName == tableName && item.ReportFileName == columnName).FirstOrDefault();
            if (dbFileNameEntity != null)
            {
                return dbFileNameEntity.DBFileName;
            }
            else if(tableName.EndsWith("异议标注"))
            {
                 dbFileNameEntity = MapList.
                 Where(item => item.ReportTableName == "异议标注" && item.ReportFileName == columnName).FirstOrDefault();
                if (dbFileNameEntity != null)
                {
                    return dbFileNameEntity.DBFileName;
                }
            }
            else if (tableName.EndsWith("本人声明"))
            {
                dbFileNameEntity = MapList.
                Where(item => item.ReportTableName == "本人声明" && item.ReportFileName == columnName).FirstOrDefault();
                if (dbFileNameEntity != null)
                {
                    return dbFileNameEntity.DBFileName;
                }
            }
            return columnName.Trim();
        }
        public static string GetDbName(string tableName)
        {
            var dbTableName = MapList.Find(item => item.ReportTableName == tableName);

            if (dbTableName != null)
            {
                return "credit." + dbTableName.DBTableName.Trim();
            }
            else if (tableName.EndsWith("异议标注"))
            {
                dbTableName = MapList.Find(item => item.ReportTableName == "异议标注");
                if (dbTableName != null)
                {
                    return "credit." + dbTableName.DBTableName.Trim();
                }
            }
            else if (tableName.EndsWith("本人声明"))
            {
                dbTableName = MapList.Find(item => item.ReportTableName == "本人声明");
                if (dbTableName != null)
                {
                    return "credit." + dbTableName.DBTableName.Trim();
                }
            }
            return tableName;
        }


        public static IEnumerable<TableMapEntity> GetColumns(string tableName)
        {
            var dbTableName = MapList.Where(item => item.ReportTableName == tableName);

            if (dbTableName != null || dbTableName.Count() != 0)
            {
                return dbTableName;
            }
            return null;
        }

    }
}
