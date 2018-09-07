using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Dao;


namespace Vcredit.NetSpider.Service
{
    // CallsEntity服务对象
    internal class callsImpl : BaseService<CallsEntity>, ICalls
    {
        IOperationLogDao OperationLogDao = null;
        /// <summary>
        /// 根据身份证号与手机号，通过通话记录计算最长使用月份
        /// </summary>
        /// <param name="IdentityNo">身份证号</param>
        /// <param name="Mobile">手机号</param>
        /// <returns></returns>
        //public int GetUseMonthsByIdentityNoAndMobile(string IdentityNo, string Mobile)
        //{
        //    int UseMonths = 0;
        //    var ls = base.FindObjsByHql("select top 1 Start_time from CallsEntity where OperationLog.IdentityNo=? and  OperationLog.Mobile=? order by Start_time", new object[] { IdentityNo, Mobile });
        //    if (ls != null && ls.Count > 0)
        //    {
        //        object Start_time = ls[0][0];
        //        if (Start_time != null && !String.IsNullOrEmpty(Start_time.ToString()))
        //        {
        //            var starttime = DateTime.Parse(Start_time.ToString());
        //            UseMonths = Vcredit.Common.Utility.CommonFun.GetIntervalOf2DateTime(DateTime.Now, starttime, "M");
        //        }
        //    }

        //    return UseMonths;
        //}


        public IList<CallsEntity> GetListOneMonth(string IdentityNo, string Mobile)
        {
            IList<CallsEntity> entityList = new List<CallsEntity>();
            try
            {
                var opr = OperationLogDao.FindObjsByHql(@"select id,SendTime from OperationLogEntity where IdentityNo=? and  Mobile=? order by id desc", new object[] { IdentityNo, Mobile }, 1, 1).FirstOrDefault();
                if (opr != null)
                {
                    object[] objs = new object[2];
                    objs[0] = opr[0];
                    objs[1] = DateTime.Parse(opr[1].ToString()).AddMonths(-1).ToString();
                    //entityList = base.Find("from CallsEntity WHERE Oper_id=? and Start_time>=?", objs);
                    entityList = base.GetSession().CreateSQLQuery("select * from jxl.calls WITH(NOLOCK) where oper_id=" + objs[0] + " and Start_time>='" + objs[1] + "'").AddEntity("CallsEntity", typeof(CallsEntity)).List<CallsEntity>();
                    if (entityList.Count == 0)
                    {
                        entityList = base.GetSession().CreateSQLQuery("select * from jxl.calls_t WITH(NOLOCK) where oper_id=" + objs[0] + " and Start_time>='" + objs[1] + "'").AddEntity("CallsEntity", typeof(CallsEntity)).List<CallsEntity>();
                    }
                }
            }
            catch (Exception)
            { }
            return entityList;
        }


        public IList<CallsEntity> GetCallListByOprid(int opr_id)
        {
            IList<CallsEntity> entityList = new List<CallsEntity>();
            try
            {
                //object[] objs = new object[1];
                //objs[0] = opr_id;
                //entityList = base.Find("from CallsEntity WHERE Oper_id=?", objs);
                entityList = base.GetSession().CreateSQLQuery("select * from jxl.calls WITH(NOLOCK) where oper_id=" + opr_id).AddEntity("CallsEntity", typeof(CallsEntity)).List<CallsEntity>();

                if (entityList.Count == 0)
                {
                    entityList = base.GetSession().CreateSQLQuery("select * from jxl.calls_t WITH(NOLOCK) where oper_id=" + opr_id).AddEntity("CallsEntity", typeof(CallsEntity)).List<CallsEntity>();
                }
            }
            catch (Exception)
            { }
            return entityList;
        }
    }
}

