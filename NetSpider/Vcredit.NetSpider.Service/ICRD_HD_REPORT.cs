using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Service
{
    // CRD_HD_REPORTEntity服务对象接口
    public interface ICRD_HD_REPORT : IBaseService<CRD_HD_REPORTEntity> 
    {
        /// <summary>
        /// 根据Bid,查询征信报告数据
        /// </summary>
        /// <param name="bid">VBS系统业务号</param>
        /// <param name="isAll">是否包含三个统计表</param>
        /// <returns></returns>
        CRD_HD_REPORTEntity GetByBid(int bid,bool isAll=false);
        /// <summary>
        /// 根据业务信息,查询征信报告数据
        /// </summary>
        /// <param name="BusId">业务号</param>
        /// <param name="BusType">业务类别</param>
        /// <param name="isAll">是否包含三张统计表信息</param>
        /// <returns></returns>
        CRD_HD_REPORTEntity GetByBusiness(string BusId,string BusType, bool isAll = false);
        /// <summary>
        /// 根据身份证号,查询征信报告数据
        /// </summary>
        /// <param name="IdentityCard">身份证号</param>
        /// <param name="isAll">是否包含三张统计表信息</param>
        /// <returns></returns>
        CRD_HD_REPORTEntity GetByIdentityCard(string IdentityCard, bool isAll=true);
        /// <summary>
        /// 根据身份证号和业务类型,查询征信报告数据
        /// </summary>
        /// <param name="IdentityCard">身份证号</param>
        /// <param name="BusType">业务类别</param>
        /// <returns></returns>
        CRD_HD_REPORTEntity GetByIdentityCardAndBusType(string IdentityCard, string BusType, bool isAll = true);
        /// <summary>
        /// 根据身份证号和业务类型,查询征信报告所有明细数据
        /// </summary>
        /// <param name="IdentityCard">身份证号</param>
        /// <param name="BusType">业务类别</param>
        /// <returns></returns>
        CRD_HD_REPORTEntity GetDataAllByIdentityCardAndBusType(string IdentityCard,string reportsn, string BusType, bool isAll = true);
        /// <summary>
        /// 根据Bid,查询征信报告数据, 同时查询关联子表
        /// </summary>
        /// <param name="bid">VBS系统业务号</param>
        /// <returns></returns>
        CRD_HD_REPORTEntity GetWithSubByBid(int bid);
         /// <summary>
        /// 根据报告编号,获取对象
        /// </summary>
        /// <param name="ReportSn">报告编号</param>
        /// <returns></returns>
        CRD_HD_REPORTEntity GetByReportSn(string ReportSn,bool isAall=false);
        /// <summary>
        /// 查询报告是否存在
        /// </summary>
        /// <param name="Loginname"></param>
        /// <param name="ReportCreateTime"></param>
        /// <returns></returns>
        bool ReportIsExist(string Loginname, string ReportCreateTime);
        /// <summary>
        /// 获取VBS同步数据
        /// </summary>
        /// <param name="reportid"></param>
        /// <returns></returns>
        IList<object[]> GetVbsSycnData(int reportid);
        CRD_HD_REPORTEntity GetByReportId(string reportid);
    }
}

