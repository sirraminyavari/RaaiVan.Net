using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaiVan.Modules.GlobalUtilities.DBCompositeTypes
{
    public class StringPairTableType : ITableType
    {
        public StringPairTableType() : base() { } //empty constructor is a must

        public string MSSQLName { get { return "StringPairTableType"; } }

        [PgName("first_value")]
        public string FirstValue;

        [PgName("second_value")]
        public string SecondValue;

        public StringPairTableType(string firstValue, string secondValue)
        {
            FirstValue = firstValue;
            SecondValue = secondValue;
        }

        public object[] to_array()
        {
            return new List<object>() { FirstValue, SecondValue }.ToArray();
        }

        public StringPairTableType[] get_array(List<StringPairTableType> list)
        {
            return list.ToArray();
        }
    }
}
