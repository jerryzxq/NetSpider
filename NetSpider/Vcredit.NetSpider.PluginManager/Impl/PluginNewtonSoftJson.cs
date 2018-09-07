using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Vcredit.Common.Constants;

namespace Vcredit.NetSpider.PluginManager.Impl
{
    public class PluginNewtonSoftJson : IPluginJsonParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="JsonString"></param>
        /// <param name="SelectNode"></param>
        /// <returns></returns>
        public string GetResultFromParser(string JsonString, string SelectNode)
        {
            try
            {
                JObject jObject = JObject.Parse(JsonString);
                object obj = jObject[SelectNode];
                string retStr = string.Empty;
                if (obj != null)
                {
                    retStr = obj.ToString();
                }
                return retStr;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 通过多级节点，解析json字符串
        /// </summary>
        /// <param name="JsonString">json字符串源</param>
        /// <param name="SelectNode">节点树形结构，默认已":"分隔</param>
        /// <param name="Split">分隔符</param>
        /// <returns></returns>
        public string GetResultFromMultiNode(string JsonString, string SelectNodes, char Split = ':')
        {
            try
            {
                string[] arrNode = SelectNodes.Split(Split);
                JObject jObject = JObject.Parse(JsonString);
                JToken jToken = null; 
                for (int i = 0; i < arrNode.Length; i++)
                {
                    jObject = JObject.Parse(JsonString);
                    jToken = jObject[arrNode[i]];

                    if (jToken != null)
                    {
                        JsonString = jToken.ToString();
                    }
                }
                return JsonString;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// json字符串转为object对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="JsonString"></param>
        /// <returns></returns>
        public T DeserializeObject<T>(string JsonString)
        {
            JsonSerializerSettings setting = new JsonSerializerSettings();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(JsonString);
        }
        public string SerializeObject(object value, bool IsNullIgnore,string DateFormat)
        {
            string jsonString = string.Empty;
            
            JsonSerializerSettings Setting = new JsonSerializerSettings();
            Setting.Formatting = Formatting.None;

            if (!string.IsNullOrEmpty(DateFormat))
            {
                IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();
                timeFormat.DateTimeFormat = DateFormat;
                Setting.Converters.Add(timeFormat);
            }
           

            if (IsNullIgnore)
            {
                Setting.NullValueHandling = NullValueHandling.Ignore;
                jsonString = JsonConvert.SerializeObject(value, Setting);
            }
            else
            {
                jsonString = JsonConvert.SerializeObject(value, Setting);
            }
            return jsonString;
        }

        /// <summary>
        /// 把json字符串解析成Dictionary
        /// </summary>
        /// <param name="JsonString">json字符串</param>
        /// <returns></returns>
        public IDictionary<string, string> GetStringDictFromParser(string JsonString)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            try
            {
                JObject jObject = JObject.Parse(JsonString);
                foreach (KeyValuePair<string, JToken> item in jObject)
                {
                    dict.Add(item.Key, item.Value.ToString());
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return dict;
        }

        /// <summary>
        /// 通过节点获取该节点的字符串组
        /// </summary>
        /// <param name="JsonString">json字符串源</param>
        /// <param name="SelectNode">选择的节点</param>
        /// <returns></returns>
        public List<string> GetArrayFromParse(string JsonString, string SelectNode)
        {
            List<string> list = new List<string>();

            try
            {
                JObject jObject = JObject.Parse(JsonString);
                object obj = jObject[SelectNode];
                if (obj != null)
                {
                    JArray jArray = JArray.Parse(obj.ToString());
                    for (var i = 0; i < jArray.Count; i++)
                    {

                        list.Add(jArray[i].ToString());
                    }
                }
                return list;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
