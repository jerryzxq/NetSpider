﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Service
{
    // CRD_CD_LNEntity服务对象接口
    public interface ICRD_CD_LN : IBaseService<CRD_CD_LNEntity> 
    {
        /// <summary>
        /// 根据Bid,查询征信报告数据
        /// </summary>
        /// <param name="bid">VBS系统业务号</param>
        /// <param name="isAll">是否包含三个统计表</param>
        /// <returns></returns>
        IList<CRD_CD_LNEntity> GetListByReportId(int reportId);
    }
}

