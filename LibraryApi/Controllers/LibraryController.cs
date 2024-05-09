using LibraryApi.Models;
using LibraryApi.Data;

using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace LibraryApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LibraryController : ControllerBase
    {
        private readonly DataContextDapperMaster _dapperAdd;
        private readonly DataContextDapperSlave _dapperRead;
        public LibraryController(IConfiguration config)
        {
            _dapperAdd = new DataContextDapperMaster(config);
            _dapperRead = new DataContextDapperSlave(config);
        }

        [HttpGet("GetCustomers")]
        public IEnumerable<Customer> GetCustomers()
        {
            string sql = @$"SELECT * FROM customer";
            return _dapperRead.LoadData<Customer>(sql);
        }

        [HttpGet("GetLibraries")]
        public IEnumerable<Library> GetLibraries()
        {
            string sql = @$"SELECT * FROM library";
            return _dapperRead.LoadData<Library>(sql);
        }

    }
}
