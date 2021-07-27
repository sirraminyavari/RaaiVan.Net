using Npgsql;
using RaaiVan.Modules.GlobalUtilities.DBCompositeTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaiVan.Modules.GlobalUtilities
{
    public interface IDBCompositeType
    {
        NpgsqlParameter toNpgSqlParameter();

        SqlParameter toMSSQLParameter(string name);
    };

    public class DBCompositeType<T> : IDBCompositeType
    {
        private List<T> Values;

        public DBCompositeType()
        {
            Values = new List<T>();
        }

        public DBCompositeType<T> add(T value)
        {
            Values.Add(value);
            return this;
        }

        public DBCompositeType<T> add(List<T> list)
        {
            Values.AddRange(list);
            return this;
        }

        public NpgsqlParameter toNpgSqlParameter()
        {
            NpgsqlParameter p = new NpgsqlParameter();
            p.Value = Values.ToArray();
            return p;
        }

        public SqlParameter toMSSQLParameter(string name)
        {
            DataTable tbl = new DataTable();

            if (!typeof(ITableType).IsAssignableFrom(typeof(T))) return null;

            ITableType emptyInstance = ((ITableType)Activator.CreateInstance(typeof(T)));

            emptyInstance.to_array().ToList().ForEach(c => tbl.Columns.Add());

            Values.ForEach(v => tbl.Rows.Add(((ITableType)v).to_array()));

            SqlParameter tblParam = new SqlParameter(name, SqlDbType.Structured);
            tblParam.TypeName = "[dbo].[" + emptyInstance.MSSQLName + "]";
            tblParam.Value = tbl;

            return tblParam;
        }
    }
}
