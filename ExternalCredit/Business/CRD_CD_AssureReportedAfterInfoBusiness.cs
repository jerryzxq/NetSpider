using Amib.Threading;
using ServiceStack.OrmLite;
using System.Collections.Generic;
using System.Linq.Expressions;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
using System;
using System.Linq;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CommonLayer;

namespace Vcredit.ExtTrade.BusinessLayer
{
    /// <summary>
    /// 担保征信上报信息
    /// </summary>
    public class CRD_CD_AssureReportedAfterInfoBusiness
    {
        BaseDao dao = new BaseDao();

        /// <summary>
        /// 登陆失败导致状态为等待上传，修改状态为默认值，等待重新上传
        /// </summary>
        public void UpLoadingMarkDefault(int diffHour)
        {
            var updateDateTime = DateTime.Now.AddHours(diffHour);

            var entities = dao.Select<CRD_CD_AssureReportedAfterInfoEntity>(x => x.State == (int)SysEnums.AssureReportState.UpLoading && 
                                                                            x.UpdateTime < updateDateTime
                                                                            )
                                                                  .ToList();

            var msg = string.Format("担保后上报 --> 登陆失败导致状态为等待上传的总数为：{0}条，修改状态为默认值，等待重新上传", entities.Count);
            Log4netAdapter.WriteInfo(msg);
            Console.WriteLine(msg);

            foreach (var item in entities)
            {
                item.State = (int)SysEnums.AssureReportState.Default;
                item.StateDescription += "--登陆失败导致状态为等待上传，修改状态为默认值";

                dao.Update(item);
            }
        }
    }
}
