using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.Framework.Server.Dto;
using Vcredit.NetSpider.Emall.Entity;
using Vcredit.NetSpider.Emall.Framework;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.PluginManager;

namespace Vcredit.NetSpider.Emall.Crawler
{
    public class AliPaySendData
    {
        public static void SendKafka(AlipayBaicEntity baic)
        {
            SendAcction<AliPayData>(() =>
            {

                var data = new AliPayData();
                data.alipay_address.AddRange(baic.Address);
                data.alipay_bank.AddRange(baic.Bank);
                IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口

                baic.FlowerBill.ForEach(e =>
                {
                    if (e.ID > 0)
                    {
                        if (e.BillFlowerOrderDetail != null)
                        {
                            e.BillFlowerOrderDetail.ForEach(x =>
                            {

                                if (x.ID > 0)
                                    data.alipay_billflowerorderdetail.Add(x);

                            });
                        }
                        if (e.BillFlowerRePayment != null)
                        {
                            e.BillFlowerRePayment.ForEach(x =>
                            {
                                if (x.ID > 0)
                                    data.alipay_billflowerrepayment.Add(x);

                            });
                        }
                        e.BillFlowerOrderDetail = null;
                        e.BillFlowerRePayment = null;
                        data.alipay_billflower.Add(e);
                    }
                });
                baic.Bank = null;
                baic.Address = null;
                baic.FlowerBill = null;
                baic.Bill = null;
                baic.ElectronicBill = null;
                baic.FlowerBillDetail = null;

                data.alipay_baic.Add(baic);

                return data;
            });


        }


        public static void SendKafka(List<AlipayBillEntity> bill)
        {
            SendAcction<AliPayData>(() =>
            {
                var data = new AliPayData();
                bill.ForEach(e =>
                {
                    if (e.BillCreditPayment != null)
                    {
                        data.alipay_billcreditpayment.Add(e.BillCreditPayment);
                        e.BillCreditPayment = null;
                    }

                    if (e.BillExpenditure != null)
                    {
                        data.alipay_billexpenditure.Add(e.BillExpenditure);
                        e.BillExpenditure = null;
                    }

                    if (e.BillOrder != null)
                    {
                        e.BillOrder.BillOrderDetail.ForEach(x =>
                        {
                            data.alipay_billorderdetail.Add(x);

                        });

                        e.BillOrder.BillOrderLogistics.ForEach(x =>
                        {
                            data.alipay_billorderlogistics.Add(x);
                        });

                        e.BillOrder.BillOrderDetail = null;
                        e.BillOrder.BillOrderLogistics = null;
                        data.alipay_billorder.Add(e.BillOrder);
                        e.BillOrder = null;
                    }

                    data.alipay_billpayment.AddRange(e.BillPayment);
                    e.BillPayment = null;

                    if (e.BillRefunds != null)
                    {
                        data.alipay_billrefunds.Add(e.BillRefunds);
                        e.BillRefunds = null;
                    }

                    if (e.BillTransfer != null)
                    {
                        data.alipay_billtransfer.Add(e.BillTransfer);
                        e.BillTransfer = null;
                    }

                    data.alipay_bill.Add(e);
                });
                return data;
            });
        }

        public static void SendKafkaFinish(AlipayBaicEntity baic)
        {

            SendAcction<AliPayEnd>(() =>
            {
                var detail = new AliPayEndDetail();
                detail.token = baic.Token;
                detail.user_id = baic.UserID;
                detail.baicid = baic.ID;
                var end = new AliPayEnd() { end_flag = detail };
                return end;
            });
        }

        static void SendAcction<T>(Func<T> func)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    T result = func();
                    SendKafka<T>(new AliPayKafkaResult<T>() { data = result });
                }
                catch (Exception ex)
                {
                    Log4netAdapter.WriteError("SendAcction==>", ex);
                }
            }).ContinueWith(e => e.Dispose());
        }

        public static void SendKafka<T>(AliPayKafkaResult<T> paydata)
        {


            IPluginJsonParser jsonService = PluginServiceManager.GetJsonParserPlugin();//Json字符串解析接口
         
            IRequestCrawler crawler = new RequestCrawler();
            paydata.topic = Config.Current.SendKafkaTopic;
            var httpItem = new HttpItem()
            {

                URL = Config.Current.SendKafkaService + "/kafka/sendData",
                Postdata = jsonService.SerializeObject(paydata, true),
                Method = "post",
                PostEncoding = Encoding.UTF8,
                ContentType = "application/json;"

            };
            CrawlerResult crawlerResult = crawler.GetHtml(httpItem);
            BaseRes Res = jsonService.DeserializeObject<BaseRes>(crawlerResult.Html);
            var result = Res.StatusCode == (int)SystemEnums.ServiceStatus.Success;
            if (!result)
            {
                Log4netAdapter.WriteInfo("AliPaySendData==>" + Res.StatusDescription);
            }

        }



    }
}
