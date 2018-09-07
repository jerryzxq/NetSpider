using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.CommonLayer
{
    public static class MapperHelper
    {
        public static TDest Map<TFrom, TDest>(TFrom from)
        {
            return EmitMapper.ObjectMapperManager.DefaultInstance.GetMapper<TFrom, TDest>().Map(from);
        }

        public static void Map<TFrom, TDest>(TFrom from, TDest dest)
        {
            EmitMapper.ObjectMapperManager.DefaultInstance.GetMapper<TFrom, TDest>().Map(from, dest);
        }
    }
}
