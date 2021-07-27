using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaiVan.Modules.GlobalUtilities.DBCompositeTypes
{
    public class GuidTableType : ITableType
    {
        public string MSSQLName { get { return "GuidTableType"; } }

        [PgName("value")]
        public Guid? Value;

        public object[] to_array()
        {
            return new List<object>() { Value.HasValue ? Value : null }.ToArray();
        }

        public GuidTableType[] get_array(List<GuidTableType> list)
        {
            return list.ToArray();
        }
    }
}
