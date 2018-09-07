using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity;
using System.Threading.Tasks;
using Vcredit.NetSpider.Emall.Business.TaoBao;
using Vcredit.NetSpider.Emall.Dto.TaoBao;
using Vcredit.NetSpider.Emall.Framework;

namespace Vcredit.NetSpider.Emall.Business
{

    public class TaobaoAlipayinfoBll : Business<TaobaoAlipayinfoEntity, SqlConnectionFactory>
    {


        public static readonly TaobaoAlipayinfoBll Initialize = new TaobaoAlipayinfoBll();
        TaobaoAlipayinfoBll() { }

        public TaobaoAlipayinfoEntity Load(UInt64 id)
        {
            return Single(e => e.Id == id);
        }

        public List<TaobaoAlipayinfoEntity> List()
        {
            return Select();
        }

        public List<TaobaoAlipayinfoEntity> List(SqlExpression<TaobaoAlipayinfoEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(TaobaoAlipayinfoEntity item)
        {
            return base.Save(item);
        }

        public void SaveAsync(TaobaoAlipayinfoEntity item)
        {
            Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        }

        /// <summary>
        /// ���� token ��ȡ��������
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public dynamic QueryHuaBaiByToken(string token)
        {
            var userinfo = TaobaoUserInfoBll.Initialize.GetByToken(token);
            var resultDto = new TaoBaoHuaBaiDto();

            TaobaoAlipayinfoEntity data = null;
            if (userinfo == null ||
                (data = this.Select(x => x.Userid == userinfo.Id).FirstOrDefault()) == null)
            {
                resultDto.DataStatus = HuaBaiDataStatus.NoData;
                resultDto.DataStatusDescription = "��������û�вɼ���";
            }
            else
            {
                resultDto = MapperHelper.Map<TaobaoAlipayinfoEntity, TaoBaoHuaBaiDto>(data);
                resultDto.DataStatus = HuaBaiDataStatus.Success;
                resultDto.DataStatusDescription = "�������ݻ�ȡ�ɹ�";
            }
            return resultDto;

            //return data.Select(x => new { x.UserId, x.FlowerAvailable, x.FlowersBalance, x.FlowerCreditAuthResult, x.CreateTime })
        }
    }
}
