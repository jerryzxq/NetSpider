﻿using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vcredit.Common.Ext;
using Vcredit.ExternalCredit.AssureAdmin.Tools;
using Vcredit.ExternalCredit.Dto;
using Vcredit.ExternalCredit.Dto.Assure;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.ModelLayer.Nolmal;

namespace Vcredit.ExternalCredit.AssureAdmin.Controllers
{
    public class AssureReportAfterController : BaseController
    {
        /// <summary>
        /// 担保上报数据查询
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ReportList()
        {
            return View();
        }

        /// <summary>
        /// 数据列表
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Get_AssureReportList(AssureReportAfterQueryDto param)
        {
            var data = new DataResponseDto<List<CRD_CD_AssureReportedAfterInfoEntity>>();

            BaseDao dao = new BaseDao();
            SqlExpression<CRD_CD_AssureReportedAfterInfoEntity> sqlexp = dao.SqlExpression<CRD_CD_AssureReportedAfterInfoEntity>();
            if (!param.GuaranteeLetterCode.IsEmpty())
            {
                sqlexp.Where(x => x.GuaranteeLetterCode == param.GuaranteeLetterCode);
            }
            if (param.State != null)
            {
                sqlexp.Where(x => x.State == param.State.Value);
            }
            if (param.UpdateTimeBegin != null)
            {
                sqlexp.Where(x => x.UpdateTime >= param.UpdateTimeBegin.Value);
            }
            if (param.UpdateTimeEnd != null)
            {
                sqlexp.Where(x => x.UpdateTime < param.UpdateTimeEnd.Value);
            }
            if (!string.IsNullOrEmpty(param.CreditorName))
            {
                sqlexp.Where(x => x.CreditorName.Contains(param.CreditorName));
            }

            data.TotalCount = dao.Count(sqlexp);   // 总数

            sqlexp.OrderByDescending(x => x.Id).Skip((param.PageIndex - 1) * param.PageSize);
            sqlexp.Take(param.PageSize);
            data.Result = dao.Select(sqlexp); // 分页数据

            data.PageIndex = param.PageIndex;
            data.PageSize = param.PageSize;
            data.EndTime = DateTime.Now.ToString();
            data.StatusCode = StatusCode.Success;
            data.StatusDescription = "数据查询成功";

            return new DateJsonResult("yyyy-MM-dd HH:mm") { Data = data };
        }

        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public FileResult ExportAssureReport(AssureReportAfterQueryDto param)
        {
            BaseDao dao = new BaseDao();
            SqlExpression<CRD_CD_AssureReportedAfterInfoEntity> sqlexp = dao.SqlExpression<CRD_CD_AssureReportedAfterInfoEntity>();
            if (!param.GuaranteeLetterCode.IsEmpty())
            {
                sqlexp.Where(x => x.GuaranteeLetterCode == param.GuaranteeLetterCode);
            }
            if (param.State != null)
            {
                sqlexp.Where(x => x.State == param.State.Value);
            }
            if (param.UpdateTimeBegin != null)
            {
                sqlexp.Where(x => x.UpdateTime >= param.UpdateTimeBegin.Value);
            }
            if (param.UpdateTimeEnd != null)
            {
                sqlexp.Where(x => x.UpdateTime < param.UpdateTimeEnd.Value);
            }
            if (!string.IsNullOrEmpty(param.CreditorName))
            {
                sqlexp.Where(x => x.CreditorName.Contains(param.CreditorName));
            }

            // 最多取1000条数据
            sqlexp.OrderByDescending(x => x.Id).Take(200000);
            var data = dao.Select(sqlexp); // 分页数据


            var dt = new DataTable();
            dt.Columns.AddRange(new[] {
                                        new DataColumn { ColumnName = "担保业务编号", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "担保合同号码", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "姓名", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "身份证号", DataType = typeof(string) },

                                        new DataColumn { ColumnName = "担保起始日期", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "担保到期日期", DataType = typeof(string) },

                                        new DataColumn { ColumnName = "担保金额", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "费率", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "在保余额（剩余本金）", DataType = typeof(string) },

                                        new DataColumn { ColumnName = "代偿日期", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "代偿金额", DataType = typeof(string) },

                                        new DataColumn { ColumnName = "状态", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "描述", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "上传日期", DataType = typeof(string) },

                                        new DataColumn { ColumnName = "放款方", DataType = typeof(string) },

                                        new DataColumn { ColumnName = "主合同编号", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "主合同号码", DataType = typeof(string) },
                                    }
                                );
            foreach (var item in data)
            {
                dt.Rows.Add(new object[] { item.GuaranteeLetterCode ,
                    item.GuaranteeContractCode,
                    item.WarranteeName,
                    item.WarranteeCertNo,
                    item.GuaranteeStartDate.ToString("yyyy-MM-dd"),
                    item.GuaranteeStopDate.ToString("yyyy-MM-dd"),
                    item.GuaranteeSum,
                    item.Rate,
                    item.InkeepBalance,

                    item.BalanceTransferDate.ToString("yyyy-MM-dd"),
                    item.BalanceTransferSum,

                    item.State,
                    item.StateDescription,
                    item.UpdateTime,

                    item.CreditorName,

                    item.MainCreditorCode,
                    item.MainContractCode
                });
            }

            //第一种:使用FileContentResult
            byte[] fileContents = Vcredit.ExtTrade.CommonLayer.NPOIHelper.x2007.TableToExcelBytes(dt, "数据导出");
            return File(fileContents, "application/ms-excel", string.Format("担保赔付后新增开户_数据导出_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHss")));

            ////第二种:使用FileStreamResult
            //var fileStream = new MemoryStream(fileContents);
            //return File(fileStream, "application/ms-excel", "fileStream.xls");

            ////第三种:使用FilePathResult
            ////服务器上首先必须要有这个Excel文件,然会通过Server.MapPath获取路径返回.
            //var fileName = Server.MapPath("~/Files/fileName.xls");
            //return File(fileName, "application/ms-excel", "fileName.xls");
        }

    }
}