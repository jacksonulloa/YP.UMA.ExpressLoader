using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using YP.ZReg.Repositories.Interfaces;
using YP.ZReg.Utils.Helpers;
using YP.ZReg.Utils.Interfaces;

namespace YP.ZReg.Repositories.Implementations
{
    public class BaseRepository(IDependencyProviderService _dps) : IBaseRepository
    {
        private readonly IDependencyProviderService dps = _dps;
        public async Task<int> EjecutarNonQuerySqlAsync(string sql, IEnumerable<SqlParameter>? parametros = null, CancellationToken ct = default)
        {
            string key = "4sb4nc-T1-734m-1ng3r14SW";
            string cadConex = ToolHelper.DesencriptarString(key, dps.dbc.ConnectionString);

            using SqlConnection cn = new(cadConex);
            await cn.OpenAsync(ct);
            using SqlCommand cmd = new(sql, cn) { CommandType = CommandType.Text };
            if (parametros != null)
            {
                foreach (var p in parametros)
                    cmd.Parameters.Add(p);
            }
            return await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<int> EjecutarNonQuerySpAsync(string storedName, IEnumerable<SqlParameter>? parametros = null, CancellationToken ct = default)
        {
            int value;
            string key = "4sb4nc-T1-734m-1ng3r14SW";
            //string cadConex = ToolHelper.DesencriptarString(key, dps.dbc.ConnectionString);
            //try
            //{
                using SqlConnection cn = new(dps.dbc.ConnectionString);
                await cn.OpenAsync(ct);

                using SqlCommand cmd = new(storedName, cn) { CommandType = CommandType.StoredProcedure };
                if (parametros != null)
                {
                    foreach (var p in parametros)
                        cmd.Parameters.Add(p);
                }
                value = await cmd.ExecuteNonQueryAsync(ct);
            //}
            //catch (Exception ex)
            //{
            //    throw;
            //}
            return value;
        }
        public async Task<List<T>> EjecutarConsultaSqlAsync<T>(string query, Dictionary<string, object> parametros) where T : new()
        {
            string key = "4sb4nc-T1-734m-1ng3r14SW";
            string cadConex = ToolHelper.DesencriptarString(key, dps.dbc.ConnectionString);
            List<T> resultado = [];
            using (SqlConnection connection = new(cadConex))
            {
                await connection.OpenAsync();
                using SqlCommand command = new(query, connection);
                if (parametros != null)
                {
                    foreach (var param in parametros)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }
                using var reader = await command.ExecuteReaderAsync();
                resultado = MapReaderToList<T>(reader);
            }
            return resultado;
        }
        public async Task<List<T>> EjecutarConsultaSpAsync<T>(string storedName,
            IEnumerable<SqlParameter>? parametros,
            CancellationToken ct = default) where T : new()
        {
            //string key = "4sb4nc-T1-734m-1ng3r14SW";
            //string cadConex = ToolHelper.DesencriptarString(key, dps.dbc.ConnectionString);

            //using SqlConnection connection = new(cadConex);
            using SqlConnection connection = new(dps.dbc.ConnectionString);
            await connection.OpenAsync(ct);

            using SqlCommand command = new(storedName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parametros != null)
            {
                foreach (var p in parametros)
                    command.Parameters.Add(p);
            }

            using SqlDataReader reader = await command.ExecuteReaderAsync(ct);
            return MapReaderToList<T>(reader);
        }
        public async Task<List<T>> EjecutarConsultaEscalarSqlAsync<T>(string query, Dictionary<string, object> parametros)
        {
            string key = "4sb4nc-T1-734m-1ng3r14SW";
            string cadConex = ToolHelper.DesencriptarString(key, dps.dbc.ConnectionString);
            List<T> resultado = [];
            using SqlConnection connection = new(cadConex);
            await connection.OpenAsync();
            using SqlCommand command = new(query, connection);

            if (parametros != null)
            {
                foreach (var param in parametros)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                // Lee la primera columna como tipo T
                resultado.Add((T)Convert.ChangeType(reader[0], typeof(T)));
            }
            return resultado;
        }
        public async Task<List<T>> EjecutarConsultaEscalarSpAsync<T>(string storedName,
            IEnumerable<SqlParameter>? parametros)
        {
            string key = "4sb4nc-T1-734m-1ng3r14SW";
            string cadConex = ToolHelper.DesencriptarString(key, dps.dbc.ConnectionString);

            List<T> resultado = [];

            using SqlConnection connection = new(cadConex);
            await connection.OpenAsync();

            using SqlCommand command = new(storedName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parametros != null)
            {
                foreach (var p in parametros)
                    command.Parameters.Add(p);
            }

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                resultado.Add(ConvertFromDb<T>(reader.GetValue(0))!);
            }

            return resultado;
        }
        public async Task<T?> EjecutarConsultaUnicaSqlAsync<T>(string query, Dictionary<string, object> parametros) where T : new()
        {
            string key = "4sb4nc-T1-734m-1ng3r14SW";
            string cadConex = ToolHelper.DesencriptarString(key, dps.dbc.ConnectionString);
            using SqlConnection connection = new(cadConex);
            await connection.OpenAsync();
            using SqlCommand command = new(query, connection);

            if (parametros != null)
            {
                foreach (var param in parametros)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var properties = typeof(T).GetProperties();
                T instance = new();
                foreach (var prop in properties)
                {
                    if (!HasColumn(reader, prop.Name) || reader[prop.Name] == DBNull.Value)
                        continue;

                    var value = reader[prop.Name];
                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    var safeValue = Convert.ChangeType(value, targetType);
                    prop.SetValue(instance, safeValue);
                }
                return instance;
            }
            return default; // Retorna null si no se encontró nada
        }
        public async Task<T?> EjecutarConsultaUnicaSpAsync<T>(string storedName,
            IEnumerable<SqlParameter>? parametros) where T : new()
        {
            string key = "4sb4nc-T1-734m-1ng3r14SW";
            string cadConex = ToolHelper.DesencriptarString(key, dps.dbc.ConnectionString);

            using SqlConnection connection = new(cadConex);
            await connection.OpenAsync();

            using SqlCommand command = new(storedName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parametros != null)
            {
                foreach (var p in parametros)
                    command.Parameters.Add(p);
            }

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var properties = typeof(T).GetProperties();
                T instance = new();

                foreach (var prop in properties)
                {
                    if (!HasColumn(reader, prop.Name)) continue;

                    object value = reader[prop.Name];
                    if (value is DBNull) continue;

                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    var safeValue = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                    prop.SetValue(instance, safeValue);
                }

                return instance;
            }

            return default; // null si no hay filas
        }

        public async Task<List<TParent>> EjecutarConsultaJoinSqlAsync<TParent, TChild, TKey>(
            string sql,
            Func<SqlDataReader, TParent> mapParent,
            Func<SqlDataReader, TChild> mapChild,
            Func<TParent, List<TChild>> childCollection,
            Func<TParent, TKey> keyParent,
            CancellationToken ct = default)
            where TKey : notnull
        {
            var result = new Dictionary<TKey, TParent>();
            using SqlConnection cn = new(dps.dbc.ConnectionString);
            await cn.OpenAsync(ct);

            using SqlCommand cmd = new(sql, cn);
            using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                // Mapear parent
                var parent = mapParent(reader);
                var parentKey = keyParent(parent);

                if (!result.TryGetValue(parentKey, out var parentExistente))
                {
                    parentExistente = parent;
                    result.Add(parentKey, parentExistente);
                }

                // Mapear child
                var child = mapChild(reader);

                // Agregar hijo a la colección del padre
                childCollection(parentExistente).Add(child);
            }

            return result.Values.ToList();
        }

        //public async Task<List<TParent>> EjecutarConsultaJoinSpAsync<TParent, TChild, TKey>(
        //    string storedName,
        //    IEnumerable<SqlParameter>? parametros,
        //    Func<SqlDataReader, TParent> mapParent,
        //    Func<SqlDataReader, TChild> mapChild,
        //    Func<TParent, List<TChild>> childCollection,
        //    Func<TParent, TKey> keyParent,
        //    CancellationToken ct = default)
        //    where TKey : notnull
        //{
        //    var result = new Dictionary<TKey, TParent>();
        //    using SqlConnection cn = new(dps.dbc.ConnectionString);
        //    await cn.OpenAsync(ct);

        //    using SqlCommand cmd = new(storedName, cn) { CommandType = CommandType.StoredProcedure };

        //    if (parametros != null)
        //    {
        //        foreach (var p in parametros)
        //            cmd.Parameters.Add(p);
        //    }

        //    using var reader = await cmd.ExecuteReaderAsync(ct);

        //    while (await reader.ReadAsync(ct))
        //    {
        //        // Construir parent
        //        var parent = mapParent(reader);
        //        var parentKey = keyParent(parent);

        //        if (!result.TryGetValue(parentKey, out var parentExistente))
        //        {
        //            parentExistente = parent;
        //            result.Add(parentKey, parentExistente);
        //        }

        //        // Construir child
        //        var child = mapChild(reader);
        //        // Vincular hijo
        //        childCollection(parentExistente).Add(child);
        //    }
        //    return result.Values.ToList();
        //}
        public async Task<List<TParent>> EjecutarConsultaJoinSpAsync<TParent, TChild, TKey>(
            string storedName,
            IEnumerable<SqlParameter>? parametros,
            Func<SqlDataReader, TParent> mapParent,
            Func<SqlDataReader, TChild> mapChild,
            Func<TParent, List<TChild>> childCollection,
            Func<TParent, TKey> keyParent,
            CancellationToken ct = default)
            where TKey : notnull
            where TParent : class
            where TChild : class
        {
            var result = new Dictionary<TKey, TParent>();
            using SqlConnection connection = new(dps.dbc.ConnectionString);
            await connection.OpenAsync(ct);

            using SqlCommand command = new(storedName, connection) { CommandType = CommandType.StoredProcedure };
            if (parametros != null)
            {
                foreach (var p in parametros)
                    command.Parameters.Add(p);
            }

            using var reader = await command.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                var parent = mapParent(reader);
                var parentKey = keyParent(parent);

                if (!result.TryGetValue(parentKey, out var parentExistente))
                {
                    parentExistente = parent;
                    result.Add(parentKey, parentExistente);
                }

                var child = mapChild(reader);
                childCollection(parentExistente).Add(child);
            }

            return result.Values.ToList();
        }
        private List<T> MapReaderToList<T>(SqlDataReader reader) where T : new()
        {
            var results = new List<T>();
            var properties = typeof(T).GetProperties();

            while (reader.Read())
            {
                T instance = new();
                foreach (var prop in properties)
                {
                    if (!HasColumn(reader, prop.Name) || reader[prop.Name] == DBNull.Value)
                        continue;
                    var value = reader[prop.Name];
                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    var safeValue = Convert.ChangeType(value, targetType);
                    prop.SetValue(instance, safeValue);
                }
                results.Add(instance);
            }
            return results;
        }
        private bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        private static T? ConvertFromDb<T>(object value)
        {
            if (value is DBNull) return default;
            var t = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            return (T)Convert.ChangeType(value, t, CultureInfo.InvariantCulture);
        }
        public SqlParameter SetParameter(string name, SqlDbType type, object? value, int size = 0,
                                 ParameterDirection direction = ParameterDirection.Input,
                                 byte precision = 0, byte scale = 0)
        {
            var p = new SqlParameter(name, type)
            {
                Direction = direction,
                Value = value ?? DBNull.Value
            };
            if (size > 0) p.Size = size;
            if (type == SqlDbType.Decimal && precision > 0)
            {
                p.Precision = precision;
                p.Scale = scale;
            }
            return p;
        }
    }
}
