using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaiVan.Modules.GlobalUtilities.DBCompositeTypes
{
    public class GuidStringTableType : ITableType
    {
        public GuidStringTableType() : base() { } //empty constructor is a must

        public string MSSQLName { get { return "GuidStringTableType"; } }

        [PgName("first_value")]
        public Guid? FirstValue;

        [PgName("second_value")]
        public string SecondValue;

        public GuidStringTableType(Guid? firstValue, string secondValue)
        {
            FirstValue = firstValue;
            SecondValue = secondValue;
        }

        public object[] to_array()
        {
            return new List<object>() { FirstValue, SecondValue }.ToArray();
        }

        public GuidStringTableType[] get_array(List<GuidStringTableType> list)
        {
            return list.ToArray();
        }
    }
}
