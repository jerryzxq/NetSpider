using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.Mongo.Faceverify;

namespace Vcredit.NetSpider.DataAccess.Mongo
{
    public class FaceverifyMongo
    {
        string faceCompareTable = "faceverify_compare";
         BaseMongo baseMongo = null;
         public FaceverifyMongo()
        {
            baseMongo = new BaseMongo("netspider");
        }
         public void SaveFaceCompare(FaceCompare faceCompare)
         {
             try
             {
                 baseMongo.Insert<FaceCompare>(faceCompare, faceCompareTable);
             }
             catch (Exception e)
             {
                 throw new Exception(e.Message);
             }
         }
    }
}
