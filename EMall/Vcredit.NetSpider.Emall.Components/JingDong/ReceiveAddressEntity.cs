
using Newtonsoft.Json;
using ServiceStack.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("JD_ReceiveAddresse")]

    public class ReceiveAddressEntity : BaseEntity //收货地址信息
    {
        /// <summary>
        /// 收货人
        /// </summary>
        [JsonProperty(PropertyName = "name", NullValueHandling = NullValueHandling.Ignore)]
        public string Receiver { get; set; }

        /// <summary>
        /// 所在地区
        /// </summary>

        public string Area { get; set; }
        [JsonProperty(PropertyName = "where", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// 地址
        /// </summary>

        public string Address { get; set; }

        /// <summary>
        /// 手机
        /// </summary>
        [JsonProperty(PropertyName = "mobile", NullValueHandling = NullValueHandling.Ignore)]
        public string Mobile { get; set; }

        /// <summary>
        /// 固定电话
        /// </summary>
        [JsonProperty(PropertyName = "fax", NullValueHandling = NullValueHandling.Ignore)]
        public string Telephone { get; set; }

        /// <summary>
        /// 电子邮箱
        /// </summary>
        [JsonProperty(PropertyName = "email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

    }
}
