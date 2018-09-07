using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using Vcredit.Common.Utility;

namespace Vcredit.NetSpider.PluginManager.Impl
{
    public class PluginHtmlAgilityPack : IPluginHtmlParser
    {
        public List<string> GetResultFromParser(string InputString, string SelectRegulation, string SelectAttribute,bool IsClearFlag)
        {
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDoc = new HtmlDocument();
            HtmlNodeCollection htmlnodes = null;
            List<string> results = new List<string>();
            if (String.IsNullOrEmpty(SelectAttribute))
            {
                SelectAttribute = "innerhtml";
            }
            try
            {
                HtmlNode.ElementsFlags.Remove("form");
                HtmlNode.ElementsFlags.Remove("option");

                InputString = IsClearFlag ? CommonFun.ClearFlag(InputString) : InputString;
                htmlDoc.LoadHtml(InputString);
                htmlnodes = htmlDoc.DocumentNode.SelectNodes(SelectRegulation);
                if (htmlnodes != null)
                {
                    foreach (HtmlNode nodeItem in htmlnodes)
                    {
                        if (SelectAttribute.ToLower() == "outerhtml" || SelectAttribute.ToLower() == "outer")
                        {
                            results.Add(nodeItem.OuterHtml);
                        }
                        else if (SelectAttribute.ToLower() == "innerhtml" || SelectAttribute.ToLower() == "inner")
                        {
                            results.Add(nodeItem.InnerHtml);
                        }
                        else if (SelectAttribute.ToLower() == "innertext" || SelectAttribute.ToLower() == "text")
                        {
                            results.Add(nodeItem.InnerText);
                        }
                        else 
                        {
                            if (nodeItem.Attributes[SelectAttribute] != null)
                            {
                                results.Add(nodeItem.Attributes[SelectAttribute].Value);
                            }
                        }
                      
                    }
                }

                return results;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
