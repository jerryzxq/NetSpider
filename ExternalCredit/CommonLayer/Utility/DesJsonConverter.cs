using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Ext;
namespace Vcredit.ExternalCredit.CommonLayer.Utility
{
    public  class DesJsonConverter:JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object value = null;
            if (reader.Value == null)
                return value;
            if (objectType == typeof(int?))
            {
                value = reader.Value.ToString().Replace(",", "").ToInt();
            }
            else if (objectType == typeof(decimal?) )
            {
                value = reader.Value.ToString().Replace(",", "").ToDecimal();
            }
            else if (objectType == typeof(DateTime?) )
            {
                value = reader.Value.ToString().Replace(",", "").ToDateTime();
            }
            else if(objectType==typeof(byte?))
            {
                value = reader.Value.ToString().Replace(",", "").ToByte();
            }
            return value;
        }
        /// <summary>
        /// 判断是否为Bool类型
        /// </summary>
        /// <param name="objectType">类型</param>
        /// <returns>为bool类型则可以进行转换</returns>
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
           
        }
    }
}
