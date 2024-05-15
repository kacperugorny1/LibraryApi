
using Dapper;
using Npgsql;
namespace LibraryApi.Data
{
    public class DataContextDapperMaster
    {
        private readonly IConfiguration _config;
        public DataContextDapperMaster(IConfiguration config)
        {
            _config = config;
        }
        public bool ExecuteSql(string sql)
        {
            NpgsqlConnection connection = new NpgsqlConnection(_config.GetConnectionString("MasterConnection"));
            return connection.Execute(sql) != 0;
        }
        public int ExecuteSqlWithRowCount(string sql)
        {
            NpgsqlConnection connection = new NpgsqlConnection(_config.GetConnectionString("MasterConnection"));
            return connection.Execute(sql);
        }
        public T? LoadDataFirstOrDefault<T>(string sql)
        {
            NpgsqlConnection connection = new NpgsqlConnection(_config.GetConnectionString("SlaveConnection"));
            return connection.QueryFirstOrDefault<T>(sql);
        }
    }
}
