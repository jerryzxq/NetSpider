/*********************************************  
 * * 功能描述： 解析json数据的相关接口
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
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.PluginManager
{
    public interface IPluginJsonParser
    {
        /// <summary>
        /// 获取json解析后的数据
        /// </summary>
        /// <param name="JsonString">json字符串</param>
        /// <param name="SelectNode">选择的节点</param>
        /// <returns></returns>
        string GetResultFromParser(string JsonString, string SelectNode);
        /// <summary>
        /// 通过多级节点，解析json字符串
        /// </summary>
        /// <param name="JsonString">json字符串源</param>
        /// <param name="SelectNode">节点树形结构，默认已":"分隔</param>
        /// <param name="Split">分隔符</param>
        /// <returns></returns>
        string GetResultFromMultiNode(string JsonString, string SelectNodes, char Split = ':');
        T DeserializeObject<T>(string JsonString);
        /// <summary>
        /// 对象转成json字符串
        /// </summary>
        /// <param name="value">对象</param>
        /// <param name="IsNullIgnore">是否过滤为空属性</param>
        /// <param name="DateFormat">设置日期类型格式</param>
        /// <returns></returns>
        string SerializeObject(object value, bool IsNullIgnore = false, string DateFormat = Consts.DateFormatString11);
        /// <summary>
        /// 把json字符串解析成Dictionary
        /// </summary>
        /// <param name="JsonString">json字符串</param>
        /// <returns></returns>
        IDictionary<string, string> GetStringDictFromParser(string JsonString);
        /// <summary>
        /// 通过节点获取该节点的字符串组
        /// </summary>
        /// <param name="JsonString">json字符串源</param>
        /// <param name="SelectNode">选择的节点</param>
        /// <returns></returns>
        List<string> GetArrayFromParse(string JsonString, string SelectNode);
    }
}
