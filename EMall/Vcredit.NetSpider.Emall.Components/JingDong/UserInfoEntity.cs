using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using System.Linq;
using System.Text;
using Vcredit.Common.Ext;
namespace   Vcredit.NetSpider.Emall.Entity 
{
    [Alias("JD_UserInfo")]
    
    public class UserInfoEntity
    {
  
        [Ignore]
        public DateTime CreateTime { get; set; }
        [AutoIncrement]
        [PrimaryKey]
        public int ID { get; set; }
        /// <summary>
        /// 会话令牌
        /// </summary>
     
        public string Token { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
 
        public string Sex { get; set; }


        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? Birthday { get; set; }   
      

        /// <summary>
        /// 兴趣集合
        /// </summary>
 
        public string  HobbyList { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Emile { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string TrueName { get; set; }

        /// <summary>
        /// 所在地
        /// </summary>
    
        public string Address { get; set; }

        /// <summary>
        /// 会员等级
        /// </summary>
        public string MemberLevel { get; set; }


        /// <summary>
        /// 级别有效期
        /// </summary>
      
        public string LevelValidity { get; set; }
        /// <summary>
        /// 成长值
        /// </summary>        
        public int GrowthValue { get; set; }



        /// <summary>
        /// 会员类型
        /// </summary>
  
        public string MemberType { get; set; }


        /// <summary>
        /// 注册时间  设置mapping  RegisDate
        /// </summary>
        [Alias("RegistDate")]
        public DateTime? RegistDateReal { get; set; }

        /// <summary>
        /// 注册省份
        /// </summary>
        public string RegistProvince { get; set; }

        /// <summary>
        /// 注册城市
        /// </summary>
  
        public string RegistCity { get; set; }

        /// <summary>
        /// 验证手机号
        /// </summary>

        public string Phone { get; set; }

        /// <summary>
        /// 是否绑定QQ
        /// </summary>
   
        public string  IsBindQQ { get; set; }

        /// <summary>
        /// 是否绑定微信
        /// </summary>
    
        public string  IsBindWChart { get; set; }
        /// <summary>
        /// 近一年实际消费金额
        /// </summary>
        public decimal LastYearCost { get; set; }
        /// <summary>
        /// 会员级别变动记录
        /// </summary>
        public string  MemberLevelChangeRecord  { get; set; }

        /// <summary>
        /// 认证姓名
        /// </summary>
        public string AuthName { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public string IdentityCard  { get; set; }
    }
}
