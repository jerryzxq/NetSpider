using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Vcredit.NetSpider.Entity.Mongo.ProvidentFund;

namespace Vcredit.NetSpider.DataAccess.Mongo
{
    public class SheBaoProvidentFundMongo
    {
 
        BaseMongo baseMongo = null;
        public SheBaoProvidentFundMongo()
        {
            baseMongo = new BaseMongo("netspider");
        }
        public void SaveCityFormSetting(City city)
        {
            //city = new City();
            //city.CityCode = "ah_anqing";
            //city.CityName = "安庆";
            //FormSetting setting = new FormSetting();
            //setting.Description = "公积金登录";
            //setting.HasVerCode = 1;
            //setting.LoginType = 1;
            //FormParam param = new FormParam();
            //param.ParameterName = "身份证号";
            //param.ParameterCode = "identitycard";
            //param.ParameterMessage = "身份证不能为空";
            //setting.FormParams.Add(param);
            //param = new FormParam();
            //param.ParameterName = "密码";
            //param.ParameterCode = "password";
            //param.ParameterMessage = "密码不能为空";
            //setting.FormParams.Add(param);
            //param = new FormParam();
            //param.ParameterName = "验证码";
            //param.ParameterCode = "vercode";
            //param.ParameterMessage = "验证码不能为空";
            //setting.FormParams.Add(param);
            //city.FormSettings.Add(setting);

            try
            {
                baseMongo.Insert<City>(city, "socialsecurity_formseting");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public void UpdateCityFormSetting(City city)
        {
            try
            {

                var query = new QueryDocument { { "CityCode", city.CityCode } };
                baseMongo.Remove<City>(query, "socialsecurity_formseting");
                SaveCityFormSetting(city);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public City GetCityFormSetting(string cityCode)
        {
            City city = null;
            try
            {
                var query = Query.And(Query.EQ("CityCode", cityCode), Query.EQ("IsUse", 1));
                city = baseMongo.FindOne<City>(query, "socialsecurity_formseting");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return city;
        }
    }
}
