using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;


namespace Vcredit.NetSpider.Service
{
    // OperationLogEntity服务对象接口
    public interface IOperationLog : IBaseService<OperationLogEntity>
    {
        /// <summary>
        /// 更新OperationLog及其对应子表信息
        /// </summary>
        /// <param name="Entity"></param>
        /// <returns></returns>
        void UpdateWithSub(OperationLogEntity Entity);
        /// <summary>
        /// 根据Bid,查询操作日志
        /// </summary>
        /// <param name="bid">VBS系统业务号</param>
        /// <returns></returns>
        OperationLogEntity GetByBid(int bid);
        /// <summary>
        /// 根据身份证号,查询操作日志
        /// </summary>
        /// <param name="IdentityNo">身份证号</param>
        /// <param name="Mobile">手机号</param>
        /// <returns></returns>
        OperationLogEntity GetByIdentityNoAndMobile(string IdentityNo, string Mobile);
        /// <summary>
        /// 根据身份证号,查询操作日志
        /// </summary>
        /// <param name="IdentityNo">身份证号</param>
        /// <param name="Name">姓名</param>
        /// <param name="Mobile">手机号</param>
        /// <returns></returns>
        OperationLogEntity GetByIdentityNoAndNameAndMobile(string IdentityNo, string Name, string Mobile);
        /// <summary>
        /// 根据业务信息,查询操作日志
        /// </summary>
        /// <param name="BusId">业务号</param>
        /// <param name="BusType">业务类别</param>
        /// <returns></returns>
        OperationLogEntity GetByBusiness(string BusId, string BusType);
        /// <summary>
        /// 获取前10条等待接收的数据
        /// </summary>
        /// <returns></returns>
        IList<OperationLogEntity> GetListFromWaitReceive();

        /// <summary>
        /// 获取前200条等待接收的数据
        /// </summary>
        /// <returns></returns>
        IList<OperationLogEntity> GetListFromWaitReceiveBySource(string source);

        /// <summary>
        /// 获取路径为空的推送报告
        /// </summary>
        /// <returns></returns>
        IList<OperationLogEntity> GetListFilePathIsNull();
        /// <summary>
        /// 更新接受数据状态
        /// </summary>
        /// <param name="Bid"></param>
        /// <param name="ReceiveFilePath"></param>
        /// <returns></returns>
        int UpdateReceiveByBid(int Bid, string ReceiveFilePath);
        /// <summary>
        /// 获取当天失败的请求
        /// </summary>
        /// <returns></returns>
        IList<OperationLogEntity> GetListTodayFailBySource(string source);
        /// <summary>
        /// 获取当天失败的请求
        /// </summary>
        /// <returns></returns>
        IList<OperationLogEntity> GetListTodayFail();
    }
}

