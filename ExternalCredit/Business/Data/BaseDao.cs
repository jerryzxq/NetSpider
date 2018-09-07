using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Vcredit.ExtTrade.BusinessLayer
{
    public class BaseDao
    {
        SqlServerHelper sqlHelper;
        public BaseDao()
        {
            sqlHelper = new SqlServerHelper("ExternalTradeDB");
        }
        public  BaseDao(string conName)
        {
            sqlHelper = new SqlServerHelper(conName);
        }
        public IDbConnection Open()
        {
            return sqlHelper.ConnectFactory.Open();
        }

        #region 查询

        public List<T> Select<T>()
        {
            using (var db = Open())
            {
                return db.Select<T>();
            }

        }

        public List<T> Select<T>(SqlExpression<T> expression)
        {
            using (var db = Open())
            {
                return db.Select<T>(expression);
            }
        }

        public List<T> Select<T>(String sql)
        {
            using (var db = Open())
            {
                return db.Select<T>(sql);
            }
        }
      
        public List<T> Select<T>(Expression<Func<T, bool>> expression)
        {
            using (var db = Open())
            {
                return db.Select<T>(expression);
            }
        }

        public List<T> Select<T>(String sql, IEnumerable<IDataParameter> paras)
        {
            using (var db = Open())
            {
                return db.Select<T>(sql, paras);
            }
        }

        public T Single<T>(Expression<Func<T, bool>> expression)
        {
            using (var db = Open())
            {
                return db.Single(expression);
            }
        }


        public T Single<T>(SqlExpression<T> expression)
        {
            using (var db = Open())
            {
                return db.Single(expression);
            }
        }

        //public T Single<T>(Func<SqlExpression<T>, SqlExpression<T>> func)
        //{
        //    using (var db = Open())
        //    {
        //        return db.Single(func);
        //    }
        //}
        public T SingleById<T>(object id)
        {
            using (var db = Open())
            {
                return db.SingleById<T>(id);
            }
        }



        public List<T> SelectByIds<T>(IEnumerable<object> ids)
        {
            using (var db = Open())
            {
                return db.SelectByIds<T>(ids);
            }
        }

        #endregion

        #region 增删改

      
        public bool Update<T>(T item)
        {
            using (var db = Open())
            {
                return db.Update<T>(item) > 0;
            }
        }

        public bool Update<T>(T item, Expression<Func<T, bool>> expression)
        {
            using (var db = Open())
            {
                return db.Update<T>(item, expression) > 0;
            }
        }

        public bool Insert<T>(T item)
        {
            using (var db = Open())
            {
                return db.Insert(item) > 0;
            }
        }

        public bool Save<T>(T item)
        {
            using (var db = Open())
            {
                return db.Save(item);
            }
        }

        public int SaveAll<T>(IEnumerable<T> items)
        {
            using (var db = Open())
            {
                return db.SaveAll(items);
            }
        }


        public long InsertIdentity<T>(T item)
        {
            using (var db = Open())
            {
                return db.Insert(item, selectIdentity: true);
            }
        }

        public void InsertAll<T>(List<T> list)
        {
            using (var db = Open())
            {
                db.InsertAll<T>(list);
            }
        }

        public bool Delete<T>(T item)
        {
            using (var db = Open())
            {
                return db.Delete(item) > 0;
            }
        }

        public bool Delete<T>(Expression<Func<T, bool>> where)
        {
            using (var db = Open())
            {
                return db.Delete(where) > 0;
            }
        }

        public bool DeleteById<T>(object id)
        {
            using (var db = Open())
            {
                return db.DeleteById<T>(id) > 0;
            }
        }
        #endregion

        #region 其他
        public string GetLastSql()
        {
            using (var db = Open())
            {
                return db.GetLastSql();
            }
        }

        public SqlExpression<T> SqlExpression<T>()
        {
            return sqlHelper.Provider.SqlExpression<T>();
        }

        public virtual long Count<T>(SqlExpression<T> expression)
        {
            using (var db = Open())
            {
                return db.Count(expression);
            }

        }

        public virtual long Count<T>(Expression<Func<T,bool>> expression)
        {
            using (var db = Open())
            {
                return db.Count(expression);
            }

        }

        #endregion
    }
}
