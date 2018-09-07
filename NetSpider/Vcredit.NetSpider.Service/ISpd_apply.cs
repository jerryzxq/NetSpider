using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;


namespace Vcredit.NetSpider.Service
{
    // Spd_LoginEntity服务对象接口
    public interface ISpd_apply : IBaseService<Spd_applyEntity>
    {
        /// <summary>
        /// 根据身份证号与抓取类型，获取采集申请信息
        /// </summary>
        /// <param name="IdentityCard">身份证号</param>
        /// <param name="SpiderType">抓取类型</param>
        /// <returns></returns>
        Spd_applyEntity GetByIdentityCardAndSpiderType(string IdentityCard, string SpiderType);
        /// <summary>
        /// 根据身份证号与抓取类型，获取采集申请信息
        /// </summary>
        /// <param name="IdentityCard">身份证号</param>
        /// <param name="SpiderType">抓取类型</param>
        /// <returns></returns>
        Spd_applyEntity GetByIdentityCardAndSpiderTypeAndMobile(string IdentityCard, string SpiderType, string Mobile);
        /// <summary>
        /// 根据身份证号与采集网站，获取采集申请信息
        /// </summary>
        /// <param name="IdentityCard">身份证号</param>
        /// <param name="Website">采集网站</param>
        /// <returns></returns>
        Spd_applyEntity GetByIdentityCardAndWebsite(string IdentityCard, string Website);
        /// <summary>
        /// 根据token，获取采集申请信息
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        Spd_applyEntity GetByToken(string Token);

        /// <summary>
        /// 根据身份证号与手机号获取采集申请信息
        /// </summary>
        /// <param name="IdentityCard">身份证号</param>
        /// <param name="SpiderType">抓取类型</param>
        /// <returns></returns>
        Spd_applyEntity GetByIdentityCardAndMobile(string IdentityCard, string Mobile, string applyStatus = "0");

        /// <summary>
        /// 根据身份证号、姓名与手机号获取采集申请信息
        /// </summary>
        /// <param name="IdentityCard">身份证号</param>
        /// <param name="SpiderType">抓取类型</param>
        /// <returns></returns>
        Spd_applyEntity GetMobileSpiderByIdentityCardAndMobileAndName(string IdentityCard, string Name, string Mobile);

        /// <summary>
        /// 根据抓取状态，获取前size条采集申请信息
        /// </summary>
        /// <param name="size"></param>
        /// <param name="crawlStatus"></param>
        /// <returns></returns>
        IList<Spd_applyEntity> GetApplyListByCrawlStatus(int size, int crawlStatus);

        /// <summary>
        /// 获取采集申请信息
        /// </summary>
        /// <param name="dic">IdentityCard(身份证号)，Name（姓名）, Mobile(手机号)，Token</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        List<Spd_applyEntity> GetApplyPageList(Dictionary<string, string> dic, int pageIndex, int pageSize);

        /// <summary>
        /// 获取时间段内的采集申请信息
        /// </summary>
        /// <param name="size"></param>
        /// <param name="crawlStatus"></param>
        /// <returns></returns>
        IList<Spd_applyEntity> GetApplyListByTimeQuantums(DateTime startDate, DateTime endDate);
    }
}

