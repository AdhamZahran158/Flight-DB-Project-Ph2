using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Flight_Reservation_App
{
    internal class Repository<T> where T : class
    {
        private readonly string connectionString = GlobalUsing.connectionString;

        public Repository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<List<T>> GetAsync(string[]? columns = null, Where[]? conditions = null)
        {
            var results = new List<T>();

            var query = "SELECT ";

            query += (columns == null || columns.Length == 0)
                ? "*"
                : string.Join(", ", columns);

            query += $" FROM {typeof(T).Name}";

            if (conditions != null && conditions.Length > 0)
            {
                query += " WHERE ";

                for (int i = 0; i < conditions.Length; i++)
                {
                    query += $"{conditions[i].Column} {conditions[i].Operator} @p{i}";

                    if (i < conditions.Length - 1)
                        query += " AND ";
                }
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                // parameters
                if (conditions != null)
                {
                    for (int i = 0; i < conditions.Length; i++)
                    {
                        cmd.Parameters.AddWithValue("@p" + i, conditions[i].Value);
                    }
                }

                await conn.OpenAsync();

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        T obj = Activator.CreateInstance<T>();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string columnName = reader.GetName(i);
                            object value = reader.GetValue(i);

                            PropertyInfo prop = typeof(T).GetProperty(columnName);

                            if (prop != null && value != DBNull.Value)
                            {
                                prop.SetValue(obj, value);
                            }
                        }

                        results.Add(obj);
                    }
                }
            }

            return results;
        }

        public async Task AddAsync(string[] columns, object[] values)
        {
            var query = $"INSERT INTO {typeof(T).Name} ";

            query += "(" + string.Join(", ", columns) + ") VALUES (";

            for (int i = 0; i < values.Length; i++)
            {
                query += "@p" + i;

                if (i < values.Length - 1)
                    query += ", ";
            }

            query += ")";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                for (int i = 0; i < values.Length; i++)
                {
                    cmd.Parameters.AddWithValue("@p" + i, values[i]);
                }

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateAsync(string[] columns, object[] values, Where[] conditions)
        {
            var query = $"UPDATE {typeof(T).Name} SET ";

            for (int i = 0; i < columns.Length; i++)
            {
                query += $"{columns[i]} = @p{i}";

                if (i < columns.Length - 1)
                    query += ", ";
            }

            if (conditions != null && conditions.Length > 0)
            {
                query += " WHERE ";

                for (int i = 0; i < conditions.Length; i++)
                {
                    query += $"{conditions[i].Column} {conditions[i].Operator} @w{i}";

                    if (i < conditions.Length - 1)
                        query += " AND ";
                }
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                for (int i = 0; i < values.Length; i++)
                {
                    cmd.Parameters.AddWithValue("@p" + i, values[i]);
                }

                if (conditions != null)
                {
                    for (int i = 0; i < conditions.Length; i++)
                    {
                        cmd.Parameters.AddWithValue("@w" + i, conditions[i].Value);
                    }
                }

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAsync(Where[] conditions)
        {
            var query = $"DELETE FROM {typeof(T).Name}";

            if (conditions != null && conditions.Length > 0)
            {
                query += " WHERE ";

                for (int i = 0; i < conditions.Length; i++)
                {
                    query += $"{conditions[i].Column} {conditions[i].Operator} @p{i}";

                    if (i < conditions.Length - 1)
                        query += " AND ";
                }
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (conditions != null)
                {
                    for (int i = 0; i < conditions.Length; i++)
                    {
                        cmd.Parameters.AddWithValue("@p" + i, conditions[i].Value);
                    }
                }

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
