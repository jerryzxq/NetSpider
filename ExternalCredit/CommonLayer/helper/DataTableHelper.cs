using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.ComponentModel;
namespace Vcredit.ExtTrade.CommonLayer
{
    /// <summary>
    /// DataTable操作
    /// </summary>
    public  class DataTableHelper
    {
        public static DataTable  CreateDataTable(Dictionary<string,Type> columnDatas)
        {
            DataTable dt=new DataTable ();
            foreach (var  item in columnDatas)
            {
                dt.Columns.Add(new DataColumn(item.Key,item.Value));
            }
            return dt;

        }
      
      
   
        /// <summary>
        /// 返回两个表的关联数据，关联后的表中只包含第一个表的字段和第二个表需要的字段
        /// </summary>
        /// <param name="FirstTable">第一个表（左表）</param>
        /// <param name="SecondTable">第二个表（右表）</param>
        /// <param name="FJC">第一个表要与第二个表关联的字段</param>
        /// <param name="SJC">第二个表要与第一个表关联的字段</param>
        /// <param name="SJCNeed">第二个表中需要保留的字段</param>
        /// <param name="IsLeftOuter">是否是左外连接，否则为内连接</param>
        /// <returns></returns>
        private static DataTable LeftOuterOrJoin(DataTable FirstTable, DataTable SecondTable, DataColumn[] FJC, DataColumn[] SJC, DataColumn[] SJCNeed, bool IsLeftOuter)
        {

            //创建一个新的DataTable

            DataTable table = new DataTable("LJoin");
            // Use a DataSet to leverage DataRelation
            using (DataSet ds = new DataSet())
            {

                //把DataTable Copy到DataSet中
                //ds.Tables.AddRange(new DataTable[]{First.Copy(),Second.Copy()});
                DataTable First = FirstTable.Copy();
                DataTable Second = SecondTable.Copy();
                First.TableName = "FirstTable";
                Second.TableName = "SecondTable";
                
                for (int i = 0; i < Second.Columns.Count; i++)//删除第二个表中不需要的字段
                {
                    bool Conten = false;
                    for (int j = 0; j < SJC.Length; j++)
                    {
                        if (SJC[j].ColumnName.ToUpper().Trim() == Second.Columns[i].ColumnName.ToUpper().Trim())
                        {
                            Conten = true;
                            break;
                        }
                    }
                    if (!Conten)
                    {
                        for (int j = 0; j < SJCNeed.Length; j++)
                        {
                            if (SJCNeed[j].ColumnName.ToUpper().Trim() == Second.Columns[i].ColumnName.ToUpper().Trim())
                            {
                                Conten = true;
                                break;
                            }
                        }
                    }
                    if (!Conten)
                    {
                        Second.Columns.RemoveAt(i);
                        i--;
                    }
                }
                ds.Tables.AddRange(new DataTable[] { First, Second });
                DataColumn[] parentcolumns = new DataColumn[FJC.Length];
                for (int i = 0; i < parentcolumns.Length; i++)
                {
                    parentcolumns[i] = ds.Tables[0].Columns[FJC[i].ColumnName];
                }

                DataColumn[] childcolumns = new DataColumn[SJC.Length];
                for (int i = 0; i < childcolumns.Length; i++)
                {
                    childcolumns[i] = ds.Tables[1].Columns[SJC[i].ColumnName];
                }

                //创建关联
                DataRelation r = new DataRelation(string.Empty, parentcolumns, childcolumns, false);
                ds.Relations.Add(r);
                //为关联表创建列
                for (int i = 0; i < First.Columns.Count; i++)
                {
                    table.Columns.Add(First.Columns[i].ColumnName, First.Columns[i].DataType);
                }

                for (int i = 0; i < Second.Columns.Count; i++)
                {
                    //看看有没有重复的列，如果有在第二个DataTable的Column的列明后加_Second
                    if (!table.Columns.Contains(Second.Columns[i].ColumnName))
                    {
                        table.Columns.Add(Second.Columns[i].ColumnName, Second.Columns[i].DataType);
                    }
                    else
                    {
                        table.Columns.Add(Second.Columns[i].ColumnName + "_Second", Second.Columns[i].DataType);
                    }
                }
                table.BeginLoadData();
                foreach (DataRow firstrow in ds.Tables[0].Rows)
                {
                    //得到行的数据
                    DataRow[] childrows = firstrow.GetChildRows(r);
                    if (childrows != null && childrows.Length > 0)
                    {
                        object[] parentarray = firstrow.ItemArray;
                        foreach (DataRow secondrow in childrows)
                        {
                            object[] secondarray = secondrow.ItemArray;
                            object[] joinarray = new object[parentarray.Length + secondarray.Length];
                            Array.Copy(parentarray, 0, joinarray, 0, parentarray.Length);
                            Array.Copy(secondarray, 0, joinarray, parentarray.Length, secondarray.Length);
                            table.LoadDataRow(joinarray, true);
                        }
                    }
                    else
                    {
                        if (IsLeftOuter)
                        {
                            object[] parentarray = firstrow.ItemArray;
                            DataRow secondrow = Second.NewRow();
                            {
                                object[] secondarray = secondrow.ItemArray;
                                object[] joinarray = new object[parentarray.Length + secondarray.Length];
                                Array.Copy(parentarray, 0, joinarray, 0, parentarray.Length);
                                Array.Copy(secondarray, 0, joinarray, parentarray.Length, secondarray.Length);
                                table.LoadDataRow(joinarray, true);
                            }
                        }
                    }
                }
                table.EndLoadData();
            }
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].ColumnName.EndsWith("_Second"))
                {
                    table.Columns.RemoveAt(i);
                    i--;
                }
            }
            return table;

        }
        /// <summary>
        /// 返回两个表的关联数据
        /// </summary>
        /// <param name="FirstTable">第一个表（左表）</param>
        /// <param name="SecondTable">第二个表（右表）</param>
        /// <param name="FJC">第一个表要与第二个表关联的字段</param>
        /// <param name="SJC">第二个表要与第一个表关联的字段</param>
        /// <param name="IsLeftOuter">是否是左外连接，否则为内连接</param>
        /// <returns></returns>
        private static DataTable LeftOuterOrJoin(DataTable FirstTable, DataTable SecondTable, DataColumn[] FJC, DataColumn[] SJC, bool IsLeftOuter)
        {

            //创建一个新的DataTable

            DataTable table = new DataTable("Join");
            // Use a DataSet to leverage DataRelation
            using (DataSet ds = new DataSet())
            {

                //把DataTable Copy到DataSet中
                //ds.Tables.AddRange(new DataTable[]{First.Copy(),Second.Copy()});

                DataTable First = FirstTable.Copy();
                DataTable Second = SecondTable.Copy();
                First.TableName = "FirstTable";
                Second.TableName = "SecondTable";
                ds.Tables.AddRange(new DataTable[] { First, Second });
                DataColumn[] parentcolumns = new DataColumn[FJC.Length];
                for (int i = 0; i < parentcolumns.Length; i++)
                {
                    parentcolumns[i] = ds.Tables[0].Columns[FJC[i].ColumnName];
                }

                DataColumn[] childcolumns = new DataColumn[SJC.Length];
                for (int i = 0; i < childcolumns.Length; i++)
                {
                    childcolumns[i] = ds.Tables[1].Columns[SJC[i].ColumnName];
                }

                //创建关联
                DataRelation r = new DataRelation(string.Empty, parentcolumns, childcolumns, false);
                ds.Relations.Add(r);
                //为关联表创建列
                for (int i = 0; i < First.Columns.Count; i++)
                {
                    table.Columns.Add(First.Columns[i].ColumnName, First.Columns[i].DataType);
                }

                for (int i = 0; i < Second.Columns.Count; i++)
                {
                    //看看有没有重复的列，如果有在第二个DataTable的Column的列明后加_Second
                    if (!table.Columns.Contains(Second.Columns[i].ColumnName))
                    {
                        table.Columns.Add(Second.Columns[i].ColumnName, Second.Columns[i].DataType);
                    }
                    else
                    {
                        table.Columns.Add(Second.Columns[i].ColumnName + "_Second", Second.Columns[i].DataType);
                    }
                }
                table.BeginLoadData();
                foreach (DataRow firstrow in ds.Tables[0].Rows)
                {
                    //得到行的数据
                    DataRow[] childrows = firstrow.GetChildRows(r);
                    if (childrows != null && childrows.Length > 0)
                    {
                        object[] parentarray = firstrow.ItemArray;
                        foreach (DataRow secondrow in childrows)
                        {
                            object[] secondarray = secondrow.ItemArray;
                            object[] joinarray = new object[parentarray.Length + secondarray.Length];
                            Array.Copy(parentarray, 0, joinarray, 0, parentarray.Length);
                            Array.Copy(secondarray, 0, joinarray, parentarray.Length, secondarray.Length);
                            table.LoadDataRow(joinarray, true);
                        }
                    }
                    else if (IsLeftOuter)
                    {
                            object[] parentarray = firstrow.ItemArray;
                            DataRow secondrow = Second.NewRow();
                            {
                                object[] secondarray = secondrow.ItemArray;
                                object[] joinarray = new object[parentarray.Length + secondarray.Length];
                                Array.Copy(parentarray, 0, joinarray, 0, parentarray.Length);
                                Array.Copy(secondarray, 0, joinarray, parentarray.Length, secondarray.Length);
                                table.LoadDataRow(joinarray, true);
                            }
                    }
                }
                table.EndLoadData();
            }
            return table;

        }
       /// <summary>
       /// 
       /// </summary>
       /// <param name="FirstTable"></param>
       /// <param name="SecondTable"></param>
       /// <param name="FJC"></param>
       /// <param name="SJC"></param>
       /// <returns></returns>
        public static DataTable Join(DataTable FirstTable, DataTable SecondTable, DataColumn[] FJC, DataColumn[] SJC)
        {
            return LeftOuterOrJoin(FirstTable, SecondTable, FJC, SJC, false);
        }
        public static DataTable Join(DataTable First, DataTable Second, DataColumn FJC, DataColumn SJC)
        {
            return Join(First, Second, new DataColumn[] { FJC }, new DataColumn[] { SJC });
        }
     

       
        public static DataTable Join(DataTable FirstTable, DataTable SecondTable, DataColumn[] FJC, DataColumn[] SJC, DataColumn[] SJCNeed)
        {
            return LeftOuterOrJoin(FirstTable, SecondTable, FJC, SJC, SJCNeed, false );
        }
   
     

        public static DataTable LeftOuterJoin(DataTable FirstTable, DataTable SecondTable, DataColumn[] FJC, DataColumn[] SJC)
        {
            return LeftOuterOrJoin(FirstTable, SecondTable, FJC, SJC, true );
        }
    
      
        public static DataTable LeftOuterJoin(DataTable FirstTable, DataTable SecondTable, DataColumn[] FJC, DataColumn[] SJC, DataColumn[] SJCNeed)
        {
            return LeftOuterOrJoin(FirstTable, SecondTable, FJC, SJC, SJCNeed, true);
        }
 
       

        
        private static bool RowEqual(DataRow rowA, DataRow rowB, DataColumnCollection columns)
        {
          //  bool result = true;
            for (int i = 0; i < columns.Count; i++)
            {
                //result &= ColumnEqual(rowA[columns[i].ColumnName], rowB[columns[i].ColumnName]);
                if (!ColumnEqual(rowA[columns[i].ColumnName], rowB[columns[i].ColumnName]))
                {
                    return false;
                }
            }
            return true ;
        }

        private static bool ColumnEqual(object objectA, object objectB)
        {
            if (objectA == DBNull.Value && objectB == DBNull.Value)
            {
                return true;
            }
            if (objectA == DBNull.Value || objectB == DBNull.Value)
            {
                return false;
            }
            return (objectA.Equals(objectB));
        }


     

   

        

    }
    public class LinqList<T> : IEnumerable<T>, IEnumerable
    {
        IEnumerable items;
        internal LinqList(IEnumerable items)
        {
            this.items = items;
        }
        #region IEnumerable<DataRow> Members
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            foreach (T item in items) yield return item;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerable<T> ie = this;
            return ie.GetEnumerator();
        }
        #endregion

    }

   
}
