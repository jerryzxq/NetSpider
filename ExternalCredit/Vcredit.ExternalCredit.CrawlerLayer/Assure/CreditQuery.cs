using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExternalCredit.Dto;
using Vcredit.ExternalCredit.Dto.Assure;

namespace Vcredit.ExternalCredit.CrawlerLayer.Assure
{
    public interface CreditQuery
    {
        /// <summary>
        /// 登陆
        /// </summary>
        /// <returns></returns>
        ApiResultDto<AssureLoginResultDto> Login();

        /// <summary>
        /// 添加信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        ApiResultDto<AddQueryInfoResultDto> AddQueryInfo(AssureQueryUserInfoParamDto param);

        /// <summary>
        /// 提交到担保平台
        /// </summary>
        /// <returns></returns>
        bool CommitToQueryV2();

        /// <summary>
        /// 从担保平台下载解析
        /// </summary>
        /// <returns></returns>
        bool AnanysisDownloadCreditV2();
    }
}
