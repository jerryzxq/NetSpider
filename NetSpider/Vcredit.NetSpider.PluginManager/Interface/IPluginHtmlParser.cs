/*********************************************  
 * * 功能描述： 分析html标签相关接口
 * * 创 建 人:  张志博
 * * 日    期:  2014/9/19
 * * 修 改 人:  
 * * 修改日期: 
 * * 修改描述:  
 * * 版    本:  1.0
 * *******************************************/ 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.PluginManager
{
    public interface IPluginHtmlParser 
    {
        /// <summary>
        /// 获取html解析后的数据集合
        /// </summary>
        /// <param name="HtmlString">html字符串</param>
        /// <param name="SelectRegulation">解析规则</param>
        /// <param name="SelectAttribute">选择的属性</param>
        /// <returns></returns>
        List<string> GetResultFromParser(string HtmlString, string SelectRegulation, string SelectAttribute = "InnerHtml",bool IsClearFlag=false);
    }
}
