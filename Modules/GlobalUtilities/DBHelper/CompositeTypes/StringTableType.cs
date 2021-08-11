using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NpgsqlTypes;
using Newtonsoft.Json;

namespace RaaiVan.Modules.GlobalUtilities.DBCompositeTypes
{
    [Serializable]
    public class StringTableType : ITableType
    {
        public StringTableType() : base() { } //empty constructor is a must

        [JsonIgnore]
        public string MSSQLName { get { return "StringTableType"; } }

        [PgName("value")]
        public string Value;

        public StringTableType(string value)
        {
            Value = value;
        }

        public object[] to_array()
        {
            return new List<object>() { Value }.ToArray();
        }

        public StringTableType[] get_array(List<StringTableType> list)
        {
            return list.ToArray();
        }
    }
}
