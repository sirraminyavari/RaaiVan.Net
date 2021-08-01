using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NpgsqlTypes;

namespace RaaiVan.Modules.GlobalUtilities.DBCompositeTypes
{
    public class DocFileInfoTableType : ITableType
    {
        public DocFileInfoTableType() : base() { } //empty constructor is a must

        public string MSSQLName { get { return "DocFileInfoTableType"; } }

        [PgName("file_id")]
        public Guid? FileID;

        [PgName("file_name")]
        public string FileName;

        [PgName("extension")]
        public string Extension;

        [PgName("mime")]
        public string MIME;

        [PgName("size")]
        public long? Size;

        [PgName("owner_id")]
        public Guid? OwnerID;

        [PgName("owner_type")]
        public string OwnerType;

        public DocFileInfoTableType(Guid? fileId, string fileName, string extension,
            string mime, long? size, Guid? ownerId, string ownerType)
        {
            FileID = fileId;
            FileName = fileName;
            Extension = extension;
            MIME = mime;
            Size = size;
            OwnerID = ownerId;
            OwnerType = ownerType;
        }

        public object[] to_array()
        {
            return new List<object>() {
                FileID,
                FileName,
                Extension,
                MIME,
                Size,
                OwnerID,
                OwnerType
            }.ToArray();
        }

        public DocFileInfoTableType[] get_array(List<DocFileInfoTableType> list)
        {
            return list.ToArray();
        }
    }
}
