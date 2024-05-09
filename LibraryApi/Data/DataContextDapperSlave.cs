using Npgsql;
using Dapper;

namespace LibraryApi.Data
{
    public class DataContextDapperSlave
    {
        private readonly IConfiguration _config;
        public DataContextDapperSlave(IConfiguration config)
        {
            _config = config;
        }

        public IEnumerable<T> LoadData<T>(string sql)
        {
            NpgsqlConnection connection = new NpgsqlConnection(_config.GetConnectionString("SlaveConnection"));
            try
            {
                return connection.Query<T>(sql);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Sequence contains no elements")) return new List<T>();
                throw new Exception(ex.Message);
            }
        }

        public T LoadDataSingle<T>(string sql)
        {
            NpgsqlConnection connection = new NpgsqlConnection(_config.GetConnectionString("SlaveConnection"));
            return connection.QuerySingle<T>(sql);
        }
        public T? LoadDataFirstOrDefault<T>(string sql)
        {
            NpgsqlConnection connection = new NpgsqlConnection(_config.GetConnectionString("SlaveConnection"));
            return connection.QueryFirstOrDefault<T>(sql);
        }
    }
}
