using Microsoft.Data.SqlClient;
using System.Data;

namespace YP.ZReg.Repositories.Interfaces
{
    public interface IBaseRepository
    {
        Task<List<T>> EjecutarConsultaEscalarSpAsync<T>(string storedName, IEnumerable<SqlParameter>? parametros);
        Task<List<T>> EjecutarConsultaEscalarSqlAsync<T>(string query, Dictionary<string, object> parametros);
        Task<List<T>> EjecutarConsultaSpAsync<T>(string storedName, IEnumerable<SqlParameter>? parametros, CancellationToken ct = default) where T : new();
        Task<List<T>> EjecutarConsultaSqlAsync<T>(string query, Dictionary<string, object> parametros) where T : new();
        Task<T?> EjecutarConsultaUnicaSpAsync<T>(string storedName, IEnumerable<SqlParameter>? parametros) where T : new();
        Task<T?> EjecutarConsultaUnicaSqlAsync<T>(string query, Dictionary<string, object> parametros) where T : new();
        Task<int> EjecutarNonQuerySpAsync(string storedName, IEnumerable<SqlParameter>? parametros = null, CancellationToken ct = default);
        Task<int> EjecutarNonQuerySqlAsync(string sql, IEnumerable<SqlParameter>? parametros = null, CancellationToken ct = default);

        Task<List<TParent>> EjecutarConsultaJoinSqlAsync<TParent, TChild, TKey>(
            string sql,
            Func<SqlDataReader, TParent> mapParent,
            Func<SqlDataReader, TChild> mapChild,
            Func<TParent, List<TChild>> childCollection,
            Func<TParent, TKey> keyParent,
            CancellationToken ct = default)
            where TKey : notnull;
        Task<List<TParent>> EjecutarConsultaJoinSpAsync<TParent, TChild, TKey>(
            string storedName,
            IEnumerable<SqlParameter>? parametros,
            Func<SqlDataReader, TParent> mapParent,
            Func<SqlDataReader, TChild> mapChild,
            Func<TParent, List<TChild>> childCollection,
            Func<TParent, TKey> keyParent,
            CancellationToken ct = default)
            where TKey : notnull
            where TParent : class
            where TChild : class;
        //Task<List<TParent>> EjecutarConsultaJoinSpAsync<TParent, TChild, TKey>(
        //    string storedName,
        //    IEnumerable<SqlParameter>? parametros,
        //    Func<SqlDataReader, TParent> mapParent,
        //    Func<SqlDataReader, TChild> mapChild,
        //    Func<TParent, List<TChild>> childCollection,
        //    Func<TParent, TKey> keyParent,
        //    CancellationToken ct = default)
        //    where TKey : notnull;
        SqlParameter SetParameter(string name, SqlDbType type, object? value, int size = 0,
                                 ParameterDirection direction = ParameterDirection.Input,
                                 byte precision = 0, byte scale = 0);
    }
}