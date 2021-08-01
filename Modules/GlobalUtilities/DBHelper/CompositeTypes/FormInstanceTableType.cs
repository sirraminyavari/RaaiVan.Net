using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NpgsqlTypes;

namespace RaaiVan.Modules.GlobalUtilities.DBCompositeTypes
{
    public class FormInstanceTableType : ITableType
    {
        public FormInstanceTableType() : base() { } //empty constructor is a must

        public string MSSQLName { get { return "FormInstanceTableType"; } }

        [PgName("instance_id")]
        public Guid? InstanceID;

        [PgName("form_id")]
        public Guid? FormID;

        [PgName("owner_id")]
        public Guid? OwnerID;

        [PgName("director_id")]
        public Guid? DirectorID;

        [PgName("admin")]
        public bool? Admin;

        [PgName("is_temporary")]
        public bool? IsTemporary;


        public FormInstanceTableType(Guid? instanceId, Guid? formId, Guid? ownerId, Guid? directorId, bool? admin, bool? isTemporary)
        {
            InstanceID = instanceId;
            FormID = formId;
            OwnerID = ownerId;
            DirectorID = directorId;
            Admin = admin;
            IsTemporary = isTemporary;
        }

        public object[] to_array()
        {
            return new List<object>() {
                InstanceID,
                FormID,
                OwnerID,
                DirectorID,
                Admin,
                IsTemporary
            }.ToArray();
        }

        public FormInstanceTableType[] get_array(List<FormInstanceTableType> list)
        {
            return list.ToArray();
        }
    }
}
