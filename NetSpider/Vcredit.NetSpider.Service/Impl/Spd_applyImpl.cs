using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Entity;
using System.Text;


namespace Vcredit.NetSpider.Service
{
    // Spd_LoginEntity服务对象
    internal class Spd_applyImpl : BaseService<Spd_applyEntity>, ISpd_apply
    {

        public Spd_applyEntity GetByIdentityCardAndWebsite(string IdentityCard, string Website)
        {
            var ls = base.FindListByHql("from Spd_applyEntity where  IdentityCard=? and Website=? and Apply_status=0 order by ApplyId desc", new object[] { IdentityCard, Website }, 1, 1);

            return ls.FirstOrDefault();
        }
        public Spd_applyEntity GetByIdentityCardAndSpiderType(string IdentityCard, string SpiderType)
        {
            var ls = base.FindListByHql("from Spd_applyEntity where  IdentityCard=? and Spider_type=? and Apply_status=0 order by ApplyId desc", new object[] { IdentityCard, SpiderType }, 1, 1);

            return ls.FirstOrDefault();
        }
        public Spd_applyEntity GetByIdentityCardAndSpiderTypeAndMobile(string IdentityCard, string SpiderType, string Mobile)
        {
            var ls = base.FindListByHql("from Spd_applyEntity where  IdentityCard=? and Spider_type=? and Mobile=? and Apply_status=0 order by ApplyId desc", new object[] { IdentityCard, SpiderType, Mobile }, 1, 1);

            return ls.FirstOrDefault();
        }
        public Spd_applyEntity GetByToken(string Token)
        {
            var ls = base.FindListByHql("from Spd_applyEntity where  Token=? order by CreateTime desc", new object[] { Token }, 1, 1);

            return ls.FirstOrDefault();
        }

        public Spd_applyEntity GetByIdentityCardAndMobile(string IdentityCard, string Mobile, string applyStatus = "0")
        {
            IList<Spd_applyEntity> ls;
            if (string.IsNullOrEmpty(applyStatus))
            {
                ls = base.FindListByHql("from Spd_applyEntity where  IdentityCard=?   and (spider_type='mobile' or spider_type='jxlmobile') and Mobile=? order by ApplyId desc", new object[] { IdentityCard, Mobile }, 1, 1);
            }
            else
            {
                ls = base.FindListByHql("from Spd_applyEntity where  IdentityCard=?   and (spider_type='mobile' or spider_type='jxlmobile') and Mobile=? and Apply_status=? order by ApplyId desc", new object[] { IdentityCard, Mobile, applyStatus }, 1, 1);
            }
            return ls.FirstOrDefault();
        }

        /// <summary>
        /// 根据身份证号、姓名与手机号获取采集申请信息
        /// </summary>
        /// <param name="IdentityCard">身份证号</param>
        /// <param name="SpiderType">抓取类型</param>
        /// <returns></returns>
        public Spd_applyEntity GetMobileSpiderByIdentityCardAndMobileAndName(string IdentityCard, string Name, string Mobile)
        {
            var ls = base.FindListByHql("from Spd_applyEntity where  IdentityCard=?   and Name=?   and (spider_type='mobile' or spider_type='jxlmobile') and Mobile=? and Apply_status=0 order by ApplyId desc", new object[] { IdentityCard, Name, Mobile }, 1, 1);
            return ls.OrderByDescending(x => x.CreateTime).FirstOrDefault();
        }

        public override object Save(Spd_applyEntity entity)
        {
            object id = base.Save(entity);
            foreach (var item in entity.Spd_applyformList)
            {
                item.ApplyId = entity.ApplyId;
                base.GetSession().Save(item);
            }
            return id;
        }

        public IList<Spd_applyEntity> GetApplyListByCrawlStatus(int size, int crawlStatus)
        {
            var ls = base.FindListByHql("from Spd_applyEntity where  crawl_status=? order by ApplyId asc", new object[] { crawlStatus }, 1, size);

            return ls.ToList();
        }

        /// <summary>
        /// 获取采集申请信息
        /// </summary>
        /// <param name="dic">IdentityCard(身份证号)，Name（姓名）, Mobile(手机号)，Token</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Spd_applyEntity> GetApplyPageList(Dictionary<string, string> dic, int pageIndex, int pageSize)
        {
            StringBuilder strbuilder = new StringBuilder();
            List<string> keys = new List<string>(dic.Keys);
            object[] objs = new object[keys.Count];
            strbuilder.Append("from Spd_applyEntity where 1=1 ");
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i].Contains("start"))
                    strbuilder.Append(" and createtime >=?");
                else if (keys[i].Contains("end"))
                    strbuilder.Append(" and createtime <?");
                else
                    strbuilder.Append(" and " + keys[i] + "=?");
                objs[i] = dic[keys[i]];
            }
            strbuilder.Append(" order by ApplyId desc");
            if (pageSize != 20)
            {
                var ls = base.FindListByHql(strbuilder.ToString(), objs, pageIndex, pageSize);
                return ls.ToList();
            }
            else
            {
                var ls = base.Find(strbuilder.ToString(), objs);
                return ls.ToList();
            }




        }

        /// <summary>
        /// 获取时间段内的采集申请信息
        /// </summary>
        /// <param name="size"></param>
        /// <param name="crawlStatus"></param>
        /// <returns></returns>
        public IList<Spd_applyEntity> GetApplyListByTimeQuantums(DateTime startDate, DateTime endDate)
        {
            StringBuilder strbuilder = new StringBuilder();
            object[] objs = new object[2];
            strbuilder.Append("from Spd_applyEntity where spider_type='mobile' and createtime >=? and createtime <? order by ApplyId desc");
            objs[0] = startDate;
            objs[1] = endDate;
            var ls = base.Find(strbuilder.ToString(), objs);
            return ls.ToList();





        }

    }
}

