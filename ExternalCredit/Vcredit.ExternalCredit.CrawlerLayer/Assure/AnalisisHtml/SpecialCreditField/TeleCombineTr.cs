using NSoup.Nodes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExternalCredit.CrawlerLayer.Assure.AnalisisHtml
{
    class TeleCombineTr : ICreditSpecialWork<Element[]>
    {

        public Dictionary<string, System.Data.DataTable> DealingSpecialWork(Dictionary<string, Element[]> dic)
        {
            Dictionary<string, System.Data.DataTable> returndic = new Dictionary<string, DataTable>();
            if (dic.Keys.Contains(CommonData.CRD_PI_TELPNT))
            {

                NomalDealing nomalDealing = new NomalDealing();
                Dictionary<string, Element[]> dicDe = new Dictionary<string, Element[]>();
                Element[] elearr = new Element[1];
                elearr[0] = dic[CommonData.CRD_PI_TELPNT][0];
                dicDe.Add(CommonData.CRD_PI_TELPNT, elearr);
                returndic = nomalDealing.DealingSpecialWork(dicDe);
                if (returndic.Count == 1 && dic[CommonData.CRD_PI_TELPNT].Length == 2)
                {
                    DataTable dt = returndic.First().Value;
                    dt.Columns.Add("Status24");
                    var ele = dic[CommonData.CRD_PI_TELPNT][1];
                    var eles = ele.GetElementsByTag(CommonData.tbody)[0].Children;
                    Dictionary<string, DataRow> dicdr = new Dictionary<string, DataRow>();
                    foreach (var item in dt.Select())
                    {
                        dicdr.Add(item["编号"].ToString(), item);
                    }
                    for (int i = 1; i < eles.Count; i++)
                    {
                        string text= eles[i].Children[0].Text();
                        var dr = dicdr[text];
                        dr["Status24"] = eles[i].Text().Substring(1).Replace(" ","");
                    }
                }
                dic.Remove(CommonData.CRD_PI_TELPNT);
            }
            return returndic;
        }
    }
}
