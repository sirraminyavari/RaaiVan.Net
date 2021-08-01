using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaiVan.Modules.GlobalUtilities.DBCompositeTypes
{
    public class BigIntTableType : ITableType
    {
        public BigIntTableType() : base() { } //empty constructor is a must

        public string MSSQLName { get { return "BigIntTableType"; } }

        [PgName("value")]
        public long? Value;

        public BigIntTableType(long? value)
        {
            Value = value;
        }

        public object[] to_array()
        {
            return new List<object>() { Value.HasValue ? Value : null }.ToArray();
        }

        public BigIntTableType[] get_array(List<BigIntTableType> list)
        {
            return list.ToArray();
        }
    }
}
