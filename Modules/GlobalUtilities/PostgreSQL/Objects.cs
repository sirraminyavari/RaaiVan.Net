using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace RaaiVan.Modules.GlobalUtilities
{
    public class RVStructuredParam
    {
        private DataTable DataTable;
        private string Name;

        public RVStructuredParam(string name)
        {
            Name = name;
            DataTable = new DataTable(name);
        }

        public RVStructuredParam add_column(String columnName, Type dataType)
        {
            try
            {
                if (dataType == typeof(int) || dataType == typeof(Int32))
                    DataTable.Columns.Add(columnName, typeof(int));
                else if (dataType == typeof(long) || dataType == typeof(Int64))
                    DataTable.Columns.Add(columnName, typeof(long));
                else if (dataType == typeof(double) || dataType == typeof(Double) || dataType == typeof(float))
                    DataTable.Columns.Add(columnName, typeof(double));
                else if (dataType == typeof(bool) || dataType == typeof(Boolean))
                    DataTable.Columns.Add(columnName, typeof(bool));
                else if (dataType == typeof(char) || dataType == typeof(Char))
                    DataTable.Columns.Add(columnName, typeof(char));
                else if (dataType == typeof(string) || dataType == typeof(String))
                    DataTable.Columns.Add(columnName, typeof(string));
                else if (dataType == typeof(DateTime))
                    DataTable.Columns.Add(columnName, typeof(DateTime));
                else if (dataType == typeof(Guid))
                    DataTable.Columns.Add(columnName, typeof(Guid));
                else if (dataType == typeof(byte[]))
                    DataTable.Columns.Add(columnName, typeof(byte[]));
            }
            catch (Exception ex)
            {
            }

            return this;
        }

        public RVStructuredParam add_row(params object[] values)
        {
            try
            {
                List<object> items = new List<object>();

                values.ToList().ForEach(v => items.Add(v));

                DataTable.Rows.Add(values.ToArray());
            }
            catch (Exception ex)
            {
            }

            return this;
        }
        /*
        public bool setParameter(int index, SQLServerPreparedStatement statement)
        {
            try
            {
                statement.setStructured(index, "[dbo].[" + this.name + "]", this.dataTable);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        */
    }

    public class RVResultSet
    {
        private List<DataTable> Tables = new List<DataTable>();
        private int ActiveSet = -1;
        private int ActiveRow = -1;

        public void add_table(NpgsqlDataReader reader)
        {
            List<string> columnNames = reader.GetColumnSchema().Select(c => c.ColumnName).ToList();

            DataTable tbl = new DataTable("tbl");

            reader.GetColumnSchema().ToList().ForEach(col =>
            {
                string colName = string.IsNullOrEmpty(col.ColumnName) ?
                    PublicMethods.random_string(5) : col.ColumnName;
                tbl.Columns.Add(colName, col.DataType);
            });

            while (reader.Read())
            {
                object[] row = new object[tbl.Columns.Count];
                int cnt = reader.GetValues(row);
                tbl.Rows.Add(row);
            }

            Tables.Add(tbl);
        }

        public int get_tables_count()
        {
            return Tables.Count;
        }

        public int get_rows_count(int tableIndex)
        {
            if (tableIndex < 0 || Tables.Count < (tableIndex + 1)) return 0;
            else return Tables[tableIndex].Rows.Count;
        }

        public int get_rows_count()
        {
            return get_rows_count(0);
        }

        public int get_columns_count(int tableIndex)
        {
            if (tableIndex < 0 || Tables.Count < (tableIndex + 1)) return 0;
            else return Tables[tableIndex].Columns.Count;
        }

        public int get_columns_count() { return get_columns_count(0); }

        public object get_value(int rowIndex, int tableIndex = 0, string columnName = null, object defaultValue = null)
        {
            if (tableIndex < 0 || rowIndex < 0 || Tables.Count < (tableIndex + 1) ||
                    Tables[tableIndex].Rows.Count < (rowIndex + 1) ||
                    !Tables[tableIndex].Columns.Contains(columnName)) return defaultValue;

            object ret = Tables[tableIndex].Rows[rowIndex][columnName];

            return ret == null ? defaultValue : ret;
        }

        public object get_value(int rowIndex, int columnIndex, int tableIndex = 0, object defaultValue = null)
        {
            if (tableIndex < 0 || Tables.Count < (tableIndex + 1) ||
                    Tables[tableIndex].Columns.Count < (columnIndex + 1)) return defaultValue;
            return get_value(rowIndex, tableIndex, Tables[tableIndex].Columns[columnIndex].ColumnName, defaultValue);
        }
    }


}
