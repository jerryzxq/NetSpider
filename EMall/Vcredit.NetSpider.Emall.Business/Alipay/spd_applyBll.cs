using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity;
using System.Threading.Tasks;
using Vcredit.NetSpider.Emall.Framework;

namespace Vcredit.NetSpider.Emall.Business
{

    public class spd_applyBll : Business<spd_applyEntity, SqlConnectionFactory>
    {


        public static readonly spd_applyBll Initialize = new spd_applyBll();

        spd_applyBll() { }
        public spd_applyEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<spd_applyEntity> List()
        {
            return Select();
        }

        public List<spd_applyEntity> List(SqlExpression<spd_applyEntity> expression)
        {
            return Select(expression);
        }

        public spd_applyEntity LoadByToken(string token)
        {
            return Single(e => e.Token == token);
        }

        public spd_applyEntity LoadStatusByToken(string token)
        {
            return Select(e => e.Token == token).OrderByDescending(t=>t.Createtime).FirstOrDefault();
        }

        public override bool Save(spd_applyEntity item)
        {
            if (item.ID == 0)
            {
                return base.Save(item);
            }
            else
            {
                return base.Update(item);
            }
        }


        /// <summary>
        /// 根据token更新抓取状态
        /// </summary>
        /// <param name="token"></param>
        /// <param name="crawlStatus"></param>
        /// <returns></returns>
        public bool UpdateCrawlStatus(string token,int crawlStatus, string description)
        {
            int relectCount = 0;
            using (var db = DBConnection)
            {
                //relectCount = db.UpdateFmt<spd_applyEntity>
                //    (set: "crawl_status=" + crawlStatus.ToString() + ", description='" + description + "'",
                //    where: " token = '" + token + "' ");
                var entity = this.Single(x => x.Token == token);
                if (entity != null)
                {
                    entity.Crawl_status = crawlStatus;
                    entity.Description = description;
                    relectCount = db.Update<spd_applyEntity>(entity);
                }
            }
            return relectCount != 0;
        }

        ///// <summary>
        ///// 根据token和限定时间获取状态实体
        ///// </summary>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //public spd_applyEntity GetByTokenAndLimitDay(string token, int limitDay)
        //{
        //    var entity = this.Select(x => x.Token == token).FirstOrDefault(x => (DateTime.Now - x.Createtime.Value).Days < limitDay);
        //    return entity;
        //}
        /// <summary>
        /// 根据token获取实体
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public spd_applyEntity GetByToken(string token)
        {
            var entity = this.Select(x => x.Token == token).FirstOrDefault();
            return entity;
        }
        /// <summary>
        /// 根据用户名和身份证号判断是否要抓取
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="idengtity">身份证号</param>
        /// <param name="limitDay">限制时间</param>
        /// <returns>true ：不能抓取，false：可以抓取</returns>
        public spd_applyEntity GetLastSinglEntity(string username, string identity, int limitDay = 0, string website="")
        {
            List<spd_applyEntity> entitys = this.Select(item => item.Login_name == username && item.Identitycard == identity &&
                (item.Crawl_status == (int)SystemEnums.CrawlStatus.数据采集成功 || item.Crawl_status == (int)SystemEnums.CrawlStatus.数据正在采集)&&item.Website==website);
            if (entitys != null && entitys.Count != 0)
            {
                var pentitys = entitys.Where(item => string.IsNullOrEmpty(item.Ptoken));
                int count = pentitys.Count();
                if (pentitys != null && count != 0)
                {
                    spd_applyEntity entity = null;
                    if (count == 1)
                    {
                        entity = pentitys.First();
                    }
                    else
                    {
                        entity = pentitys.OrderByDescending(item => item.Createtime).First();
                    }
                    if ((DateTime.Now - entity.Createtime.Value).Days <= limitDay)
                    {
                        return entity;
                    }
                }

            }
            return null;
        }
    }

}
