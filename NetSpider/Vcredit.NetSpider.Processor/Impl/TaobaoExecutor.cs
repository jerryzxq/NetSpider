using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.DataManager;
using Vcredit.NetSpider.Fetcher;
using Vcredit.NetSpider.PluginManager;
using Vcredit.NetSpider.WorkFlow;
using Vcredit.NetSpider.Entity.Service;

namespace Vcredit.NetSpider.Processor.Impl
{
    public class TaobaoExecutor : ITaobaoExecutor
    {
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();
        CookieCollection cookies = new CookieCollection();
        HttpHelper http = new HttpHelper();
        /// <summary>
        /// 获取天猫店铺的近三月成交总额
        /// </summary>
        /// <param name="SellerAddress"></param>
        /// <returns></returns>
        public TaobaoSellerRes GetTmallSellerTotalAmount(string SellerAddress)
        {
            HttpItem httpitem = null;
            HttpResult HttpRes;
            string MidStr = string.Empty;
            List<string> results = new List<string>();
            bool IsPageEnd = false;
            int page = 0;
            int count = 0;
            List<string> Prices = new List<string>();
            string nexturl = string.Empty;
            //第一步，淘宝店铺宝贝列表页，获取宝贝id
            List<string> ItemidList = new List<string>();
            page = 1;
            string price = string.Empty;//单个宝贝单个订单的价格
            decimal totalAmount = 0;//店铺销售总额
            int totalCount = 0;//店铺销售总数
            decimal OneItemTotalAmount = 0;//单个宝贝的销售总额
            int PhoneCount = 0;//单个宝贝单个订单手机专享销售数量
            int quantity = 0;//单个宝贝单个订单销售数量
            int OneItemTotalCount = 0;//单个宝贝的销售数量
            TaobaoSellerRes Res = new TaobaoSellerRes();
            TaobaoSellerMS MS = new TaobaoSellerMS();
            //添加月统计变量
            DateTime orderDate = new DateTime();
            DateTime first = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime last = first.AddMonths(-1);
            DateTime last1 = last.AddMonths(-1);
            decimal monthTotalAmount1 = 0;
            decimal monthTotalAmount2 = 0;
            decimal monthTotalAmount3 = 0;
            int monthTotalCount1 = 0;
            int monthTotalCount2 = 0;
            int monthTotalCount3 = 0;

            try
            {
                Log4netAdapter.WriteInfo("查询天猫店铺销售总额开始，店铺地址：" + SellerAddress);
                DateTime starttime = DateTime.Now;
                do
                {

                    httpitem = new HttpItem()
                    {
                        URL = SellerAddress + "/category.htm?pageNo=" + page,
                        ResultCookieType = ResultCookieType.CookieCollection,
                        CookieCollection = cookies
                    };

                    HttpRes = http.GetHtml(httpitem);
                    if (HttpRes.StatusCode != HttpStatusCode.OK)
                    {
                        IsPageEnd = true;
                    }
                    else
                    {
                        if (page == 1)
                        {
                            //公司名
                            Res.CompanyName = CommonFun.GetMidStr(CommonFun.ClearFlag(HttpRes.Html), "<label>公 司 名：</label><div class=\"right\">", "</div>");
                        }

                        results = HtmlParser.GetResultFromParser(HttpRes.Html, "//p[@class='item-not-found']", "");
                        if (results.Count > 0)
                        {
                            IsPageEnd = true;
                        }
                        else
                        {                          
                            MidStr = CommonFun.GetMidStr(HttpRes.Html, "class=\"J_TItems\"", "div class=\"comboHd\">");
                            ItemidList.AddRange(HtmlParser.GetResultFromParser(MidStr, "//dl", "data-id"));
                            var tempstrs = HtmlParser.GetResultFromParser(HttpRes.Html, "//a[@class='disable']", "");
                            if (HttpRes.Html.Contains("没找到符合条件的商品,换个条件或关键词试试吧") || (results.Count == 1 && results[0] == "下一页") || results.Count == 2)
                            {
                                IsPageEnd = true;
                            }
                        }
                        page++;
                    }
                }
                while (!IsPageEnd);

                //第二步，淘宝店铺宝贝详情页，获取订单数量、三个月订单查询链接等
                List<string> ItemAllUrlList = new List<string>();
                ItemidList = ItemidList.Distinct().ToList();
                count = ItemidList.Count;
                for (int i = 0; i < count; i++)
                {
                    //详情页主页
                    httpitem = new HttpItem()
                    {
                        URL = "http://detail.tmall.com/item.htm?id=" + ItemidList[i],
                        ResultCookieType = ResultCookieType.CookieCollection,
                        CookieCollection = cookies
                    };

                    HttpRes = http.GetHtml(httpitem);

                    if (HttpRes.StatusCode != HttpStatusCode.OK || HttpRes.Html.Contains("暂时还没有买家购买此商品。"))
                    {
                        continue;
                    }
                    MidStr = CommonFun.GetMidStr(HttpRes.Html, "<button id=\"J_listBuyerOnView\" type=\"button\" data-checkSoldNum=\"true\" class=\"J_TAjaxTrigger hidden J_TAjaxTriggerButton\" detail:params=\"", ",showBuyerList");

                    //通过查询当月订单数据，获取近三月数据的查询链接
                    httpitem = new HttpItem()
                    {
                        URL = MidStr + "&callback=jsonp1597",
                        ResultCookieType = ResultCookieType.CookieCollection,
                        CookieCollection = cookies,
                        Referer = "http://detail.tmall.com/item.htm?id=" + ItemidList[i]
                    };
                    HttpRes = http.GetHtml(httpitem);
                    if (HttpRes.StatusCode != HttpStatusCode.OK)
                    {
                        continue;
                    }
                    MidStr = CommonFun.GetMidStr(HttpRes.Html, "jsonp1597({html:\"", "\",type:\"list\"})");
                    MidStr = CommonFun.GetMidStr(MidStr, "id=\\\"J_LinkViewAll\\\" detail:params=\\\"", ",showBuyerList");
                    nexturl = MidStr.Replace("amp;", "").Replace("bidPgae=1", "");

                    //分页查询商品详情页的订单金额
                    page = 1;
                    IsPageEnd = false;
                    PhoneCount = 0;
                    OneItemTotalAmount = 0;
                    OneItemTotalCount = 0;
                    do
                    {
                        httpitem = new HttpItem()
                        {
                            URL = nexturl + "&callback=jsonp1597&bidPage=" + page,
                            ResultCookieType = ResultCookieType.CookieCollection,
                            CookieCollection = cookies,
                            Referer = "http://detail.tmall.com/item.htm?id=" + ItemidList[i]
                        };

                        HttpRes = http.GetHtml(httpitem);
                        if (HttpRes.StatusCode != HttpStatusCode.OK)
                        {
                            IsPageEnd = true;
                        }
                        else
                        {
                            MidStr = CommonFun.GetMidStr(HttpRes.Html, "jsonp1597({html:\"", "\",type:\"list\"})").Replace("\\\"", "\""); ;
                            if (MidStr.Contains("暂时还没有买家购买此宝贝"))
                            {
                                IsPageEnd = true;
                            }
                            else
                            {
                                //Prices.AddRange(HtmlParser.GetResultFromParser(MidStr, "//em", ""));
                                var tempList = HtmlParser.GetResultFromParser(MidStr, "//tr", "");

                                foreach (string strItem in tempList)
                                {
                                    // 获取此订单成交数量
                                    results = HtmlParser.GetResultFromParser(strItem, "//td[@class='quantity']", "");
                                    if (results.Count > 0)
                                    {
                                        quantity = int.Parse(results[0]);
                                        OneItemTotalCount += quantity;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    // 获取此订单成交金额
                                    results = HtmlParser.GetResultFromParser(strItem, "//em", "");
                                    if (results.Count > 0)
                                    {
                                        price = results[0];
                                    }
                                    else
                                    {
                                        // 判断是否有手机专享
                                        results = HtmlParser.GetResultFromParser(strItem, "//a[@class='tm-buy-prom']", "");
                                        if (results.Count > 0)
                                        {
                                            PhoneCount += quantity;
                                        }
                                    }

                                    //计算此订单成交金额
                                    OneItemTotalAmount += quantity * price.Replace("&yen;", "").ToDecimal(0);

                                    // 订单时间
                                    results = HtmlParser.GetResultFromParser(strItem, "//td[@class='tb-start']", "");
                                    orderDate = DateTime.Parse(results[0]);
                                    if (orderDate >= first)
                                    {
                                        monthTotalAmount1 += quantity * price.ToDecimal(0);
                                        monthTotalCount1 += quantity;
                                    }
                                    else if (orderDate < first && orderDate >= last)
                                    {
                                        monthTotalAmount2 += quantity * price.ToDecimal(0);
                                        monthTotalCount2 += quantity;
                                    }
                                    else if (orderDate < last && orderDate >= last1)
                                    {
                                        monthTotalAmount3 += quantity * price.ToDecimal(0);
                                        monthTotalCount3 += quantity;
                                    }
                                }


                                //是否包含page-end样式
                                var pageend = HtmlParser.GetResultFromParser(HttpRes.Html.Replace("\\\"", "\""), "//span[@class='page-end']", "");
                                //是否包含page-cur样式，如果没有则订单小于15个，只有第一页
                                var pagecur = HtmlParser.GetResultFromParser(HttpRes.Html.Replace("\\\"", "\""), "//span[@class='page-cur']", "");

                                if (pageend.Count > 0 || pagecur.Count == 0)
                                {
                                    IsPageEnd = true;
                                }
                            }
                            page++;
                        }
                    }
                    while (!IsPageEnd);

                    //计算手机专享金额
                    if (OneItemTotalCount > 0 && PhoneCount > 0)
                    {
                        OneItemTotalAmount += (OneItemTotalAmount / OneItemTotalCount) * PhoneCount;
                    }
                    //汇总
                    totalAmount += OneItemTotalAmount;
                    totalCount += OneItemTotalCount;
                }

                double UseMinute = 0;
                DateTime endtime = DateTime.Now;
                UseMinute = (endtime - starttime).TotalMinutes;

                Log4netAdapter.WriteInfo(string.Format("查询天猫店铺销售总额结束，店铺地址：{0},总金额：{1},总用时：{2}", SellerAddress, totalAmount, UseMinute));
                
                //月统计
                MS = new TaobaoSellerMS();
                MS.YearAndMonth = first.ToString("yyyyMM");
                MS.TotalAmount = monthTotalAmount1;
                MS.TotalCount = monthTotalCount1;
                Res.MonthStatistics.Add(MS);
                MS = new TaobaoSellerMS();
                MS.YearAndMonth = last.ToString("yyyyMM");
                MS.TotalAmount = monthTotalAmount2;
                MS.TotalCount = monthTotalCount2;
                Res.MonthStatistics.Add(MS);
                MS = new TaobaoSellerMS();
                MS.YearAndMonth = last1.ToString("yyyyMM");
                MS.TotalAmount = monthTotalAmount3;
                MS.TotalCount = monthTotalCount3;
                //总统计
                Res.MonthStatistics.Add(MS);
                Res.TotalAmount = totalAmount;
                Res.UseMinute = UseMinute;
                Res.TotalCount = totalCount;
                Res.ItemCount = ItemidList.Count;
                return Res;
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("查询淘宝店铺销售总额异常", e);
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 获取淘宝店铺的近三月成交总额
        /// </summary>
        /// <param name="SellerAddress"></param>
        /// <returns></returns>
        public TaobaoSellerRes GetTaobaoSellerTotalAmount(string SellerAddress)
        {
            HttpItem httpitem = null;
            HttpResult HttpRes;
            string MidStr = string.Empty;
            List<string> results = new List<string>();
            bool IsPageEnd = false;
            int page = 1;
            int count = 0;
            List<string> Prices = new List<string>();
            string nexturl = string.Empty;
            string price = string.Empty;//单个宝贝单个订单的价格
            decimal totalAmount = 0;//店铺销售总额
            int totalCount = 0;//店铺销售总数
            decimal OneItemTotalAmount = 0;//单个宝贝的销售总额
            int PhoneCount = 0;//单个宝贝单个订单手机专享销售数量
            int quantity = 0;//单个宝贝单个订单销售数量
            int OneItemTotalCount = 0;//单个宝贝的销售数量
            TaobaoSellerRes Res = new TaobaoSellerRes();
            TaobaoSellerMS MS = new TaobaoSellerMS();
            //添加月统计变量
            DateTime orderDate = new DateTime() ;
            DateTime first = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime last = first.AddMonths(-1);
            DateTime last1 = last.AddMonths(-1);
            decimal monthTotalAmount1 = 0;
            decimal monthTotalAmount2 = 0;
            decimal monthTotalAmount3 = 0;
            int monthTotalCount1 = 0;
            int monthTotalCount2 = 0;
            int monthTotalCount3 = 0;
            

            try
            {
                Log4netAdapter.WriteInfo(string.Format("查询淘宝店铺销售总额开始，店铺地址：{0}", SellerAddress));
                DateTime starttime = DateTime.Now;
                //第一步，淘宝店铺宝贝列表页，获取宝贝id
                List<string> ItemidList = new List<string>();
                string rateUrl = string.Empty;
                do
                {
                    httpitem = new HttpItem()
                    {
                        URL = SellerAddress + "/search.htm?pageNo=" + page,
                        ResultCookieType = ResultCookieType.CookieCollection,
                        CookieCollection = cookies
                    };

                    HttpRes = http.GetHtml(httpitem);
                    if (HttpRes.StatusCode != HttpStatusCode.OK)
                    {
                        IsPageEnd = true;
                    }
                    else
                    {
                        if (page==1)
                        {
                            results = HtmlParser.GetResultFromParser(HttpRes.Html, "//a[@data-goldlog-id='/tbwmdd.1.045']", "href");
                            if (results.Count > 0)
                            {
                                rateUrl = results[0];
                            }
                        }
                        results = HtmlParser.GetResultFromParser(HttpRes.Html, "//p[@class='item-not-found']", "");
                        if (results.Count > 0)
                        {
                            IsPageEnd = true;
                        }
                        else
                        {
                            results = HtmlParser.GetResultFromParser(HttpRes.Html, "//div[@class='shop-hesper-bd grid']", "");

                            ItemidList.AddRange(HtmlParser.GetResultFromParser(results[0], "//dl", "data-id"));
                            results = HtmlParser.GetResultFromParser(HttpRes.Html, "//div[@class='pagination']", "");
                            results = HtmlParser.GetResultFromParser(results[0], "//a[@class='disable']", "");
                            if ((results.Count == 1 && results[0] == "下一页") || results.Count == 2)
                            {
                                IsPageEnd = true;
                            }
                        }
                        page++;
                    }
                }
                while (!IsPageEnd);
                
                //查询店铺开店时间、好评率
                if (!String.IsNullOrEmpty(rateUrl))
                {
                    //详情页主页
                    httpitem = new HttpItem()
                    {
                        URL = rateUrl
                    };
                    HttpRes = http.GetHtml(httpitem);
                    results = HtmlParser.GetResultFromParser(HttpRes.Html, "//input[@id='J_showShopStartDate']", "value");
                    if (results.Count > 0)
                    {
                        Res.OpenTime =DateTime.Parse(results[0]);
                    }
                    string goodrate = CommonFun.GetMidStr(HttpRes.Html, "卖家信用评价展示 <em style=\"color:gray;\">好评率：", "%");
                    if (goodrate != "")
                    {
                        Res.GoodRate = goodrate.ToDecimal(0);
                    }
                }

                //第二步，淘宝店铺宝贝详情页，获取订单数量、三个月订单查询链接等
                ItemidList = ItemidList.Distinct().ToList();
                count = ItemidList.Count;
                for (int i = 0; i < count; i++)
                {
                    //详情页主页
                    httpitem = new HttpItem()
                    {
                        URL = "http://item.taobao.com/item.htm?id=" + ItemidList[i],
                        ResultCookieType = ResultCookieType.CookieCollection,
                        CookieCollection = cookies
                    };

                    HttpRes = http.GetHtml(httpitem);
                    if (HttpRes.StatusCode != HttpStatusCode.OK)
                    {
                        continue;
                    }
                    results = HtmlParser.GetResultFromParser(HttpRes.Html, "//button[@id='J_listBuyerOnView']", "data-api");
                    nexturl = results[0];

                    //通过查询当月订单数据，获取近三月数据的查询链接
                    httpitem = new HttpItem()
                    {
                        URL = nexturl + "&callback=jsonp1597",
                        ResultCookieType = ResultCookieType.CookieCollection,
                        CookieCollection = cookies,
                        Referer = "http://item.taobao.com/item.htm?id=" + ItemidList[i]
                    };
                    HttpRes = http.GetHtml(httpitem);
                    if (HttpRes.StatusCode != HttpStatusCode.OK || HttpRes.Html.Contains("暂时还没有买家购买此宝贝"))
                    {
                        continue;
                    }

                    MidStr = CommonFun.GetMidStr(HttpRes.Html, "jsonp1597({html:\"", "\",type:\"list\"})");
                    MidStr = CommonFun.GetMidStr(MidStr, "class=\\\"J_TAjaxTrigger\\\" detail:params=\\\"", ",showBuyerList");
                    nexturl = MidStr.Replace("amp;", "").Replace("bid_page=1", "");

                    //分页查询商品详情页的订单金额
                    page = 1;
                    IsPageEnd = false;
                    PhoneCount = 0;
                    OneItemTotalAmount = 0;
                    OneItemTotalCount = 0;
                    do
                    {

                        httpitem = new HttpItem()
                        {
                            URL = nexturl + "&callback=jsonp1597&bid_page=" + page,
                            ResultCookieType = ResultCookieType.CookieCollection,
                            CookieCollection = cookies,
                            Referer = "http://item.taobao.com/item.htm?id=" + ItemidList[i]
                        };

                        HttpRes = http.GetHtml(httpitem);
                        if (HttpRes.StatusCode != HttpStatusCode.OK)
                        {
                            IsPageEnd = true;
                        }
                        else
                        {
                            MidStr = CommonFun.GetMidStr(HttpRes.Html, "jsonp1597({html:\"", "\",type:\"list\"})").Replace("\\\"", "\"");
                            var tempList = HtmlParser.GetResultFromParser(MidStr, "//tr", "");

                            foreach (string strItem in tempList)
                            {
                                // 获取此订单成交数量
                                results = HtmlParser.GetResultFromParser(strItem, "//td[@class='tb-amount']", "");
                                if (results.Count > 0)
                                {
                                    quantity = int.Parse(results[0]);
                                    OneItemTotalCount += quantity;
                                }
                                else
                                {
                                    continue;
                                }
                                // 获取此订单成交金额
                                results = HtmlParser.GetResultFromParser(strItem, "//em[@class='tb-rmb-num']", "");
                                if (results.Count > 0)
                                {
                                    price = results[0];
                                }
                                else
                                {
                                    // 判断是否有手机专享
                                    results = HtmlParser.GetResultFromParser(strItem, "//a[@class='tm-buy-prom']", "");
                                    if (results.Count > 0)
                                    {
                                        PhoneCount += quantity;
                                    }
                                }

                                //计算此订单成交金额
                                OneItemTotalAmount += quantity * price.ToDecimal(0);

                                // 订单时间
                                results = HtmlParser.GetResultFromParser(strItem, "//td[@class='tb-start']", "");
                                orderDate =DateTime.Parse(results[0]);
                                if (orderDate >= first)
                                {
                                    monthTotalAmount1 += quantity * price.ToDecimal(0);
                                    monthTotalCount1 += quantity;
                                }
                                else if (orderDate < first && orderDate >= last)
                                {
                                    monthTotalAmount2 += quantity * price.ToDecimal(0);
                                    monthTotalCount2 += quantity;
                                }
                                else if (orderDate < last && orderDate >= last1)
                                {
                                    monthTotalAmount3 += quantity * price.ToDecimal(0);
                                    monthTotalCount3 += quantity;
                                }

                            }

                            //Prices.AddRange(HtmlParser.GetResultFromParser(MidStr.Replace("\\\"", "\""), "//em[@class='tb-rmb-num']", ""));

                            //是否包含page-end样式
                            var pageend = HtmlParser.GetResultFromParser(HttpRes.Html.Replace("\\\"", "\""), "//span[@class='page-end']", "");
                            //是否包含page-cur样式，如果没有则订单小于15个，只有第一页
                            var pagecur = HtmlParser.GetResultFromParser(HttpRes.Html.Replace("\\\"", "\""), "//span[@class='page-cur']", "");
                            if (HttpRes.Html.Contains("暂时还没有买家购买此宝贝") || pageend.Count > 0 || pagecur.Count == 0)
                            {
                                IsPageEnd = true;
                            }
                            page++;
                        }
                    }
                    while (!IsPageEnd);

                    //计算手机专享金额
                    if (OneItemTotalCount > 0 && PhoneCount > 0)
                    {
                        OneItemTotalAmount += (OneItemTotalAmount / OneItemTotalCount) * PhoneCount;
                    }
                    //汇总计算
                    totalAmount += OneItemTotalAmount;
                    totalCount += OneItemTotalCount;
                }
                double UseMinute = 0;
                DateTime endtime = DateTime.Now;
                UseMinute = (endtime - starttime).TotalMinutes;
                Log4netAdapter.WriteInfo(string.Format("查询淘宝店铺销售总额结束，店铺地址：{0},总金额：{1},总用时：{2}", SellerAddress, totalAmount, UseMinute));
                //月统计
                MS = new TaobaoSellerMS();
                MS.YearAndMonth = first.ToString("yyyyMM");
                MS.TotalAmount = monthTotalAmount1;
                MS.TotalCount = monthTotalCount1;
                Res.MonthStatistics.Add(MS);
                MS = new TaobaoSellerMS();
                MS.YearAndMonth = last.ToString("yyyyMM");
                MS.TotalAmount = monthTotalAmount2;
                MS.TotalCount = monthTotalCount2;
                Res.MonthStatistics.Add(MS);
                MS = new TaobaoSellerMS();
                MS.YearAndMonth = last1.ToString("yyyyMM");
                MS.TotalAmount = monthTotalAmount3;
                MS.TotalCount = monthTotalCount3;
                Res.MonthStatistics.Add(MS);
                //总统计
                Res.TotalAmount = totalAmount;
                Res.UseMinute = UseMinute;
                Res.TotalCount = totalCount;
                Res.ItemCount = ItemidList.Count;
                return Res;
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError("查询淘宝店铺销售总额异常", e);
                throw new Exception(e.Message);
            }
        }

    }
}
