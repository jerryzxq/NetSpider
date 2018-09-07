using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Crawler.Edu;
using Vcredit.NetSpider.Crawler.Social;
using Vcredit.NetSpider.Crawler.Edu.Chsi;
using Vcredit.NetSpider.Crawler.Mobile;
using Vcredit.NetSpider.Entity;

namespace Vcredit.NetSpider.Crawler
{
    public class CrawlerManager
    {
        /// <summary>
        /// 移动抓取接口
        /// </summary>
        /// <param name="mobileCompany">运营商</param>
        /// <param name="region">地区</param>
        /// <returns></returns>
        public static IMobileCrawler GetMobileCrawler(EnumMobileCompany mobileCompany, string region)
        {
            IMobileCrawler crawler = null;
            try
            {
                string companyName = Enum.GetName(typeof(EnumMobileCompany), mobileCompany);
                crawler = (IMobileCrawler)Common.Utility.CommonFun.CreateObject("Vcredit.NetSpider.Crawler", "Vcredit.NetSpider.Crawler.Mobile." + companyName + "." + region);

            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("MobileCrawler实例化异常", e);
            }
            return crawler;
        }

        /// <summary>
        /// 移动重置密码接口
        /// </summary>
        /// <param name="mobileCompany">运营商</param>
        /// <param name="region">地区</param>
        /// <returns></returns>
        public static IResetMobilePassWord GetResetMobilePWD(EnumMobileCompany mobileCompany, string region)
        {
            IResetMobilePassWord crawler = null;
            try
            {
                string companyName = Enum.GetName(typeof(EnumMobileCompany), mobileCompany);
                crawler = (IResetMobilePassWord)Common.Utility.CommonFun.CreateObject("Vcredit.NetSpider.Crawler", "Vcredit.NetSpider.Crawler.Mobile." + companyName + "." + region);

            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("MobileCrawler实例化异常", e);
            }
            return crawler;
        }


        /// <summary>
        /// 社保抓取接口
        /// </summary>
        /// <param name="city">城市</param>
        /// <returns></returns>
        public static ISocialSecurityCrawler GetSocialSecurityCrawler(string province, string city)
        {
            ISocialSecurityCrawler crawler = null;
            try
            {
                crawler = (ISocialSecurityCrawler)Common.Utility.CommonFun.CreateObject("Vcredit.NetSpider.Crawler", "Vcredit.NetSpider.Crawler.Social.SocialSecurity." + province + "." + city);
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("SocialSecurityCrawler实例化异常", e);
            }
            return crawler;
        }
        /// <summary>
        /// 公积金抓取接口
        /// </summary>
        /// <param name="city">城市</param>
        /// <returns></returns>
        public static IProvidentFundCrawler GetProvidentFundCrawler(string province, string city)
        {
            IProvidentFundCrawler crawler = null;
            try
            {
                crawler = (IProvidentFundCrawler)Common.Utility.CommonFun.CreateObject("Vcredit.NetSpider.Crawler", "Vcredit.NetSpider.Crawler.Social.ProvidentFund." + province + "." + city);
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("ProvidentFundCrawler实例化异常", e);
            }
            return crawler;
        }
        /// <summary>
        /// 学信网抓取接口
        /// </summary>
        /// <returns></returns>
        public static IChsiCrawler GetChsiCrawler()
        {
            IChsiCrawler crawler = null;
            try
            {
                crawler = new ChsiCrawler();
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("ChsiCrawler实例化异常", e);
            }
            return crawler;
        }
    }
}
