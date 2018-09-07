using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("Alipay_Address")]
    
    public class AlipayAddressEntity
    {

        public AlipayAddressEntity() { }


        #region Attributes

        private long iD;

        [AutoIncrement]
        /// <summary>
        /// 主键
        /// </summary>
        public long ID
        {
            get { return iD; }
            set { iD = value; }
        }

        private long? baicID;


        /// <summary>
        /// 基本信息编号
        /// </summary>
        public long? BaicID
        {
            get { return baicID; }
            set { baicID = value; }
        }

        private string name;


        /// <summary>
        /// 姓名
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string mobile;


        /// <summary>
        /// 手机
        /// </summary>
        public string Mobile
        {
            get { return mobile; }
            set { mobile = value; }
        }

        private string tel;


        /// <summary>
        /// 固话
        /// </summary>
        public string Tel
        {
            get { return tel; }
            set { tel = value; }
        }

        private string province;


        /// <summary>
        /// 
        /// </summary>
        public string Province
        {
            get { return province; }
            set { province = value; }
        }

        private string city;


        /// <summary>
        /// 
        /// </summary>
        public string City
        {
            get { return city; }
            set { city = value; }
        }

        private string area;


        /// <summary>
        /// 
        /// </summary>
        public string Area
        {
            get { return area; }
            set { area = value; }
        }

        private string address;


        /// <summary>
        /// 地址
        /// </summary>
        public string Address
        {
            get { return address; }
            set { address = value; }
        }

        private string zipCode;


        /// <summary>
        /// 邮编
        /// </summary>
        public string ZipCode
        {
            get { return zipCode; }
            set { zipCode = value; }
        }

        private DateTime? createTime;


        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime
        {
            get
            {
                if (createTime == null)
                {
                    return DateTime.Now;
                }
                else
                {
                    return createTime;
                }
            }
            set { createTime = value; }
        }
        #endregion

    }
}
