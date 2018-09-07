using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity;
using System.Threading.Tasks;
namespace Vcredit.NetSpider.Emall.Business
{

    public class AlipayAddressBll : Business<AlipayAddressEntity, SqlConnectionFactory>
    {


        public static readonly AlipayAddressBll Initialize = new AlipayAddressBll();
        AlipayAddressBll() { }

        public AlipayAddressEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayAddressEntity> List()
        {
            return Select();
        }

        public List<AlipayAddressEntity> List(SqlExpression<AlipayAddressEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(AlipayAddressEntity item)
        {
            return base.Save(item);
        }

        public void ActionSave(AlipayBaicEntity baic, List<AlipayAddressEntity> list)
        {

            if (list==null||list.Count == 0) return;
            list.ForEach(e => e.BaicID = baic.ID);
            base.Save(list);
        }


    }
}
