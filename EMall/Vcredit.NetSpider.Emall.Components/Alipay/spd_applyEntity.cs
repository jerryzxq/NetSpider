using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("spd_apply")]
    public class spd_applyEntity
    {

        public spd_applyEntity() { }


        #region Attributes

        private int applyId;

        [AutoIncrement]
        [Alias("ApplyId")]
        /// <summary>
        /// 主键ID
        /// </summary>
        public int ID
        {
            get { return applyId; }
            set { applyId = value; }
        }

        private string identitycard;


        /// <summary>
        /// 身份证号
        /// </summary>
        public string Identitycard
        {
            get { return identitycard; }
            set { identitycard = value; }
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

        public string Mobile
        {
            get { return mobile; }
            set { mobile = value; }
        }


        private string spider_type;


        /// <summary>
        /// 
        /// </summary>
        public string Spider_type
        {
            get { return spider_type; }
            set { spider_type = value; }
        }

        private string website;


        /// <summary>
        /// 采集网站
        /// </summary>
        public string Website
        {
            get { return website; }
            set { website = value; }
        }

        private string appId;
        /// <summary>
        /// 应用ID
        /// </summary>
        public string AppId
        {
            get { return appId; }
            set { appId = value; }
        }


        private string token;
        /// <summary>
        /// 客户端会话令牌
        /// </summary>
        public string Token
        {
            get { return token; }
            set { token = value; }
        }

        private string iPAddr;


        /// <summary>
        /// IP
        /// </summary>
        public string IPAddr
        {
            get { return iPAddr; }
            set { iPAddr = value; }
        }

        private DateTime? createtime;


        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? Createtime
        {
            get { return createtime; }
            set { createtime = value; }
        }

        private int? apply_status;


        /// <summary>
        /// 申请状态
        /// </summary>
        public int? Apply_status
        {
            get { return apply_status; }
            set { apply_status = value; }
        }

        private int? crawl_status;


        /// <summary>
        /// 采集状态
        /// </summary>
        public int? Crawl_status
        {
            get { return crawl_status; }
            set { crawl_status = value; }
        }

        private string description;


        /// <summary>
        /// 描述
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        private string password;

        /// <summary>
        /// 密码
        /// </summary>
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        private string login_name;
        /// <summary>
        /// 描述
        /// </summary>
        public string Login_name
        {
            get { return login_name; }
            set { login_name = value; }
        }
        private string ptoken;

        public string Ptoken
        {
            get { return ptoken; }
            set { ptoken = value; }
        }
      

        #endregion

    }
}
