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
    public class TaggedItemTableType : ITableType
    {
        public TaggedItemTableType() : base() { } //empty constructor is a must

        [JsonIgnore]
        public string MSSQLName { get { return "TaggedItemTableType"; } }

        [PgName("context_id")]
        public Guid? ContextID;

        [PgName("tagged_id")]
        public Guid? TaggedID;

        [PgName("context_type")]
        public string ContextType;

        [PgName("tagged_type")]
        public string TaggedType;

        public TaggedItemTableType(Guid? contextId, Guid? taggedId, string contextType, string taggedType)
        {
            ContextID = contextId;
            TaggedID = taggedId;
            ContextType = contextType;
            TaggedType = taggedType;
        }

        public object[] to_array()
        {
            return new List<object>() {
                ContextID,
                TaggedID,
                ContextType,
                TaggedType
            }.ToArray();
        }

        public TaggedItemTableType[] get_array(List<TaggedItemTableType> list)
        {
            return list.ToArray();
        }
    }
}
