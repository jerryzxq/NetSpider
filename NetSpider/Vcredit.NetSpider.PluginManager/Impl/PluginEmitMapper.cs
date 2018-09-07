using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmitMapper;
using EmitMapper.MappingConfiguration;

namespace Vcredit.NetSpider.PluginManager
{
    public class PluginEmitMapper
    {
        public static ObjectMapperManager objMan = new ObjectMapperManager();

        public static void Map<TFrom, TTo>(TFrom from, TTo to)
        {
            objMan.GetMapper<TFrom, TTo>().Map(from, to);
        }
        public static void DeepMap<TFrom, TTo>(TFrom from, TTo to)
        {
            objMan.GetMapper<TFrom, TTo>(new DefaultMapConfig().DeepMap()).Map(from, to);
        }

        public static void ShallowMap<TFrom, TTo>(TFrom from, TTo to)
        {
            objMan.GetMapper<TFrom, TTo>(new DefaultMapConfig().ShallowMap()).Map(from, to);
        }
    }
}
